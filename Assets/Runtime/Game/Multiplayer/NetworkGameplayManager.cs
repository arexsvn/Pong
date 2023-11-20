using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkGameplayManager
{
    public Action<int> playerScored;
    public Action serverStopped;
    public Action serverDisconnected;
    public Action<List<string>> gameStarted;
    public Action gameRestarted;
    public Action<ulong> clientDisconnected;
    private NetworkedBall _ball;
    private int _totalPlayers;
    private int _maxPlayers = 2;
    private List<Vector3> _startPositions = new List<Vector3> { new Vector3(0, 0, 9), new Vector3(0, 0, -9) };
    private string NETWORK_MANAGER_PREFAB = "Gameplay/NetworkManager";
    private string BALL_PREFAB = "Gameplay/Ball";
    private NetworkManager _networkManager;
    private UnityTransport _transport;
    private const int MESSAGE_TYPE_GAME_STARTED = 0;
    private const int MESSAGE_TYPE_PLAYER_SCORED = 1;
    private const int MESSAGE_TYPE_GAME_RESTARTED = 2;

    public NetworkGameplayManager()
    {

    }

    public void Init()
    {
        _networkManager = UnityEngine.Object.Instantiate(Resources.Load<NetworkManager>(NETWORK_MANAGER_PREFAB));
        _networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        _networkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
        _networkManager.OnServerStopped += handleServerStopped;
        _networkManager.OnTransportFailure += handleTransportFailure;

        _transport = _networkManager.NetworkConfig.NetworkTransport as UnityTransport;
    }

    private void ReceivedServerMessage(ulong clientId, FastBufferReader reader)
    {
        // Read the message type value that is written first when we send this unnamed message.
        reader.ReadValueSafe(out int messageType);
        string messageValue = null;

        switch (messageType)
        {
            case MESSAGE_TYPE_GAME_STARTED:
                reader.ReadValueSafe(out messageValue);
                List<string> uids = messageValue.Split(',').ToList<string>();
                gameStarted?.Invoke(uids);
                break;

            case MESSAGE_TYPE_PLAYER_SCORED:
                reader.ReadValueSafe(out messageValue);
                playerScored?.Invoke(int.Parse(messageValue));
                break;

            case MESSAGE_TYPE_GAME_RESTARTED:
                gameRestarted?.Invoke();
                break;
        }
    }

    private void broadcastToAllClients(int type, string message = null)
    {
        if (!_networkManager.IsServer)
        {
            Debug.LogError("Can only broadcast to clients from server!");
            return;
        }
        
        var writer = new FastBufferWriter(1100, Allocator.Temp);
        var customMessagingManager = _networkManager.CustomMessagingManager;
        // Placing the writer within a using scope assures it will be disposed upon leaving the using scope
        using (writer)
        {
            // Write our message type
            writer.WriteValueSafe(type);

            // Write our string message
            if (message != null)
            {
                writer.WriteValueSafe(message);
            }

            customMessagingManager.SendUnnamedMessageToAll(writer);
        }
    }

    public string CurrentIpAddress
    { 
        get
        {
            return _transport.ConnectionData.Address;
        }
    }

    public void SetIpAddress(string ipAddress, ushort port = 7777)
    {
        _transport.ConnectionData.Address = ipAddress;
        _transport.ConnectionData.Port = port;
    }

    public void Shutdown()
    {
        if (_networkManager.CustomMessagingManager != null)
        {
            _networkManager.CustomMessagingManager.OnUnnamedMessage -= ReceivedServerMessage;
        }

        _networkManager.Shutdown();
    }

    public void StartHost()
    {
        _networkManager.StartHost();
        _networkManager.CustomMessagingManager.OnUnnamedMessage -= ReceivedServerMessage;
        _networkManager.CustomMessagingManager.OnUnnamedMessage += ReceivedServerMessage;
    }

    public void StartClient()
    {
        _networkManager.StartClient();
        _networkManager.CustomMessagingManager.OnUnnamedMessage -= ReceivedServerMessage;
        _networkManager.CustomMessagingManager.OnUnnamedMessage += ReceivedServerMessage;
    }

    public void StartServer()
    {
        _networkManager.StartServer();
    }

    public void RestartGame()
    {
        if (!_networkManager.IsServer)
        {
            return;
        }

        List<NetworkedPlayer> players = new List<NetworkedPlayer>();
        for (int index = 0; index < _totalPlayers; index++)
        {
            ulong uid = _networkManager.ConnectedClientsIds[index];
            NetworkedPlayer player = _networkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<NetworkedPlayer>();
            players.Add(player);
        }

        createBall(players[UnityEngine.Random.Range(0, _maxPlayers)]);

        broadcastToAllClients(MESSAGE_TYPE_GAME_RESTARTED);
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (!_networkManager.IsServer)
        {
            return;
        }

        _totalPlayers++;

        if (_totalPlayers == _maxPlayers)
        {
            List<string> uids = new List<string>();
            List<NetworkedPlayer> players = new List<NetworkedPlayer>();

            for (int index = 0; index < _totalPlayers; index++)
            {
                ulong uid = _networkManager.ConnectedClientsIds[index];
                uids.Add(uid.ToString());
                NetworkObject playerObject = _networkManager.SpawnManager.GetPlayerNetworkObject(uid);
                NetworkTransform playerTransform = playerObject.GetComponent<NetworkTransform>();
                NetworkedPlayer player = playerObject.GetComponent<NetworkedPlayer>();

                players.Add(player);

                playerTransform.Teleport(_startPositions[index], playerTransform.transform.rotation, playerTransform.transform.lossyScale);

                player.Show(index);
                player.DoServe += handlePlayerServe;
            }

            createBall(players[UnityEngine.Random.Range(0, _maxPlayers)]);

            broadcastToAllClients(MESSAGE_TYPE_GAME_STARTED, String.Join(",", uids));
        }
    }

    private void handlePlayerServe()
    {
        int direction = 1;
        if (_ball.GetOwner().position.z > 0)
        {
            direction = -1;
        }

        _ball.ClearOwner();
        _ball.SetRandomVelocity(direction);
    }

    private void createBall(NetworkedPlayer player)
    {
        _ball = UnityEngine.Object.Instantiate(Resources.Load<NetworkedBall>(BALL_PREFAB));
        _ball.OutOfBounds += handleOutOfBounds;
        _ball.GetComponent<NetworkObject>().Spawn();
        _ball.SetOwner(player.GetComponent<NetworkTransform>());

        player.GetComponent<NetworkedPlayer>().CanServeClientRpc();

        NetworkTransform networkTransform = _ball.GetComponent<NetworkTransform>();
        networkTransform.Teleport(_ball.GetOwner().position, networkTransform.transform.rotation, networkTransform.transform.lossyScale);
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        clientDisconnected?.Invoke(clientId);

        if (!_networkManager.IsServer)
        {
            return;
        }

        _totalPlayers--;

        EndGame();
    }

    public void EndGame()
    {
        if (!_networkManager.IsServer)
        {
            return;
        }

        if (_ball != null)
        {
            _ball.OutOfBounds -= handleOutOfBounds;
            _ball.GetComponent<NetworkObject>().Despawn();
            _ball = null;
        }
    }

    private void handleOutOfBounds()
    {
        if (!_networkManager.IsServer)
        {
            return;
        }

        _ball.ResetToStart();

        int playerNumber = 0;
        if (_ball.transform.position.z > 0)
        {
            playerNumber = 1;
        }

        for (int index = 0; index < _totalPlayers; index++)
        {
            ulong uid = _networkManager.ConnectedClientsIds[index];
            NetworkObject playerObject = _networkManager.SpawnManager.GetPlayerNetworkObject(uid);
            NetworkedPlayer player = playerObject.GetComponent<NetworkedPlayer>();

            if (player.PlayerNumber != playerNumber)
            {
                _ball.SetOwner(playerObject.GetComponent<NetworkTransform>());
                NetworkTransform networkTransform = _ball.GetComponent<NetworkTransform>();
                networkTransform.Teleport(_ball.GetOwner().position, networkTransform.transform.rotation, networkTransform.transform.lossyScale);
                player.CanServeClientRpc();
                break;
            }
        }

        broadcastToAllClients(MESSAGE_TYPE_PLAYER_SCORED, playerNumber.ToString());
    }

    private void handleServerStopped(bool isHost)
    {
        serverStopped?.Invoke();
    }
    private void handleTransportFailure()
    {
        serverStopped?.Invoke();
    }
}