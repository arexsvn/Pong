using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;

public class NetworkGameplayManager
{
    public Action<int> playerScored;
    public Action serverStopped;
    public Action serverDisconnected;
    public Action<List<string>> gameStarted;
    public Action gameRestarted;
    public Action<ulong> clientDisconnected;
    private NetworkedBall _ball;
    private int _maxPlayers = 2;
    private List<Vector3> _startPositions = new List<Vector3> { new Vector3(0, 0, 9), new Vector3(0, 0, -9) };
    private string NETWORK_MANAGER_PREFAB = "Gameplay/NetworkManager";
    private string BALL_PREFAB = "Gameplay/NetworkedBall";
    private NetworkManager _networkManager;
    private UnityTransport _unityTransport;
    private const int MESSAGE_TYPE_GAME_STARTED = 0;
    private const int MESSAGE_TYPE_PLAYER_SCORED = 1;
    private const int MESSAGE_TYPE_GAME_RESTARTED = 2;
    private const int MESSAGE_TYPE_GAME_REQUEST_RESTART = 3;
    private ushort _port = 443;
    private string _address = "127.0.0.1";

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

        _unityTransport = _networkManager.NetworkConfig.NetworkTransport as UnityTransport;
    }

    private void ReceivedMessage(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out int messageType);
        string messageValue = "";
        try
        {
            reader.ReadValueSafe(out messageValue);
        }
        catch(Exception ex) 
        {
            // Not all messages have values.
            Debug.Log($"ReceivedServerMessage Exception :: {ex.Message}");
        }

        parseMessage(messageType, messageValue);
    }

    private void parseMessage(int messageType, string messageValue) 
    {
        switch (messageType)
        {
            case MESSAGE_TYPE_GAME_STARTED:
                List<string> uids = messageValue.Split(',').ToList<string>();
                gameStarted?.Invoke(uids);
                break;

            case MESSAGE_TYPE_PLAYER_SCORED:
                playerScored?.Invoke(int.Parse(messageValue));
                break;

            case MESSAGE_TYPE_GAME_RESTARTED:
                gameRestarted?.Invoke();
                break;

            case MESSAGE_TYPE_GAME_REQUEST_RESTART:
                RestartGame();
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

        // ensure dedicated servers receive messages.
        if (!_networkManager.IsHost)
        {
            parseMessage(type, message);
        }
    }

    private void sendMessageToServer(int type, string message = null)
    {
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

            customMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId, writer);
        }
    }

    public string CurrentIpAddress
    { 
        get
        {
            return _address;
        }
    }

    public bool IsServer
    {
        get
        {
            if (_networkManager != null)
            {
                return _networkManager.IsServer;
            }
            return false;
        }
    }

    public bool IsDedicatedServer
    {
        get
        {
            if (_networkManager != null)
            {
                return _networkManager.IsServer && !_networkManager.IsHost;
            }
            return false;
        }
    }

    public void SetIpAddress(string ipAddress, ushort port = 443)
    {
        _address = ipAddress;
        _port = port;
    }

    public void Shutdown()
    {
        if (_networkManager.CustomMessagingManager != null)
        {
            _networkManager.CustomMessagingManager.OnUnnamedMessage -= ReceivedMessage;
        }

        _networkManager.Shutdown();
    }

    public void StartHost()
    {
        _unityTransport.SetConnectionData(_address, _port);
        _networkManager.StartHost();
        _networkManager.CustomMessagingManager.OnUnnamedMessage -= ReceivedMessage;
        _networkManager.CustomMessagingManager.OnUnnamedMessage += ReceivedMessage;
    }

    public void StartClient()
    {
        _unityTransport.SetConnectionData(_address, _port);
        _networkManager.StartClient();
        _networkManager.CustomMessagingManager.OnUnnamedMessage -= ReceivedMessage;
        _networkManager.CustomMessagingManager.OnUnnamedMessage += ReceivedMessage;
    }

    public void StartServer()
    {
        _unityTransport.SetConnectionData(_address, _port);
        _networkManager.StartServer();
        _networkManager.CustomMessagingManager.OnUnnamedMessage -= ReceivedMessage;
        _networkManager.CustomMessagingManager.OnUnnamedMessage += ReceivedMessage;
    }

    public async Task<bool> StartClientWithRelay(string joinCode)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "wss"));
        NetworkManager.Singleton.GetComponent<UnityTransport>().UseWebSockets = true;
        bool result = !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();

        _networkManager.CustomMessagingManager.OnUnnamedMessage -= ReceivedMessage;
        _networkManager.CustomMessagingManager.OnUnnamedMessage += ReceivedMessage;

        return result;
    }

    public async Task<string> StartServerWithRelay(int maxConnections = 5)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "wss"));
        NetworkManager.Singleton.GetComponent<UnityTransport>().UseWebSockets = true;
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        joinCode = NetworkManager.Singleton.StartServer() ? joinCode : null;

        _networkManager.CustomMessagingManager.OnUnnamedMessage -= ReceivedMessage;
        _networkManager.CustomMessagingManager.OnUnnamedMessage += ReceivedMessage;

        return joinCode;
    }
    public async Task<string> StartHostWithRelay(int maxConnections = 5)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "wss"));
        NetworkManager.Singleton.GetComponent<UnityTransport>().UseWebSockets = true;
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        joinCode = NetworkManager.Singleton.StartHost() ? joinCode : null;

        _networkManager.CustomMessagingManager.OnUnnamedMessage -= ReceivedMessage;
        _networkManager.CustomMessagingManager.OnUnnamedMessage += ReceivedMessage;

        return joinCode;
    }

    public void RestartGame()
    {
        if (!_networkManager.IsServer)
        {
            return;
        }

        List<NetworkedPlayer> players = new List<NetworkedPlayer>();
        for (int index = 0; index < _networkManager.ConnectedClientsIds.Count; index++)
        {
            ulong uid = _networkManager.ConnectedClientsIds[index];
            NetworkedPlayer player = _networkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<NetworkedPlayer>();
            players.Add(player);
        }

        createBall(players[UnityEngine.Random.Range(0, _maxPlayers)]);

        broadcastToAllClients(MESSAGE_TYPE_GAME_RESTARTED);
    }

    public void RequestRestart()
    {
        sendMessageToServer(MESSAGE_TYPE_GAME_REQUEST_RESTART);
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

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (!_networkManager.IsServer)
        {
            return;
        }

        if (_networkManager.ConnectedClientsIds.Count == _maxPlayers)
        {
            List<string> uids = new List<string>();
            List<NetworkedPlayer> players = new List<NetworkedPlayer>();

            for (int index = 0; index < _networkManager.ConnectedClientsIds.Count; index++)
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
        // check for null in case a game just ended or disconnected.
        if (_ball == null)
        {
            return; 
        }
        
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

        EndGame();
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

        for (int index = 0; index < _networkManager.ConnectedClientsIds.Count; index++)
        {
            ulong uid = _networkManager.ConnectedClientsIds[index];
            NetworkObject playerObject = _networkManager.SpawnManager.GetPlayerNetworkObject(uid);
            NetworkedPlayer player = playerObject.GetComponent<NetworkedPlayer>();

            // Player who did not score takes posession in this version of the rules.
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