using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Action<int> playerScored;
    public Ball ballPrefab;
    private Ball _ball;
    private int _totalPlayers;
    private int _maxPlayers = 2;
    private List<Vector3> _startPositions = new List<Vector3> { new Vector3(0, 0, 9), new Vector3(0, 0, -9) };
    static string ipAddress = "127.0.0.1";

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        // Only show these buttons when not running or connected to a server.
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else if ((NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsConnectedClient) || !NetworkManager.Singleton.IsClient)
        {
            StatusLabels();
            //SubmitNewPosition();
        }

        GUILayout.EndArea();
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();

        ipAddress = GUILayout.TextField(ipAddress);

        UnityTransport transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;

        if (GUILayout.Button("Set IP Address") && !string.IsNullOrEmpty(ipAddress))
        {
            transport.ConnectionData.Address = ipAddress;
            //transport.ConnectionData.Port = 7777;
        }

        GUILayout.Label($"Current IP Address : {transport.ConnectionData.Address}");
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (_totalPlayers < _maxPlayers)
        {
            NetworkObject playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            NetworkTransform playerTransform = playerObject.GetComponent<NetworkTransform>();
            playerTransform.Teleport(_startPositions[_totalPlayers], playerTransform.transform.rotation, playerTransform.transform.lossyScale);

            _totalPlayers++;

            if (_totalPlayers == _maxPlayers)
            {
                foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<Player>().Show();
                }

                _ball = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);
                _ball.OutOfBounds += handleOutOfBounds;
                _ball.GetComponent<NetworkObject>().Spawn();
                _ball.SetRandomVelocity();
            }
        }
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        _totalPlayers--;

        if (_ball != null)
        {
            _ball.OutOfBounds -= handleOutOfBounds;
            _ball.GetComponent<NetworkObject>().Despawn();
            _ball = null;
        }
    }

    private void handleOutOfBounds(int direction)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        playerScored?.Invoke(direction);

        _ball.ResetToStart();
        _ball.SetRandomVelocity();
    }
}