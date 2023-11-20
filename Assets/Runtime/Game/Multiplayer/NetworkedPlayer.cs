using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkedPlayer : NetworkBehaviour
{
    public Action DoServe;
    public int PlayerNumber { get => _playerNumber; }
    private Mallet _mallet;
    private bool _canServe;
    private int _lastDirection = 0;
    private int _playerNumber;

    private void Awake()
    {
        _mallet = GetComponent<Mallet>();
    }

    [ClientRpc]
    public void ShowClientRpc(int playerNumber)
    {
        _playerNumber = playerNumber;
        DoShow();
    }

    [ClientRpc]
    public void CanServeClientRpc()
    {
        _canServe = true;
    }

    private void DoShow()
    {
        _mallet.Show();
    }

    public void Show(int playerNumber)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            ShowClientRpc(playerNumber);

            // Show on dedicated server as well
            if (!NetworkManager.Singleton.IsHost) 
            {
                _playerNumber = playerNumber;
                DoShow();
            }
        }
    }

    private void Update()
    {
        CheckInput();
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            _mallet.Move();
        }
    }

    private void CheckInput()
    {
        if (IsOwner)
        {
            int currentDirection = 0;

            if (_canServe)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    _canServe = false;
                    SubmitServeRequestServerRpc();
                    if (!NetworkManager.Singleton.IsHost)
                    {
                        handleServeRequest();
                    }
                    return;
                }
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                currentDirection = -1;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                currentDirection = 1;
            }

            if (currentDirection != _lastDirection)
            {
                _lastDirection = currentDirection;
                SubmitMoveRequestServerRpc(currentDirection);
            }
        }
    }

    [ServerRpc]
    private void SubmitMoveRequestServerRpc(int direction)
    {
        _mallet.SetDirection(direction);
    }

    [ServerRpc]
    private void SubmitServeRequestServerRpc()
    {
        handleServeRequest();
    }

    private void handleServeRequest()
    {
        DoServe?.Invoke();
    }
}