using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public Action DoServe;
    public int PlayerNumber { get => _playerNumber; }
    private Renderer _renderer;
    private Vector2 _bounds = new Vector2(-4, 4);
    private float _speed = .26f;
    private int _direction = 0;
    private bool _canServe;
    private int _lastDirection = 0;
    private int _playerNumber;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.enabled = false;
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
        _renderer.enabled = true;
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
            Move();
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
        _direction = direction;
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

    private void Move()
    {
        if (_direction == 0)
        {
            return;
        }
        
        float distance = _speed * _direction;
        float endX = Mathf.Clamp(transform.position.x + distance, _bounds[0], _bounds[1]);

        if (endX != transform.position.x)
        {
            transform.position = new Vector3(endX, transform.position.y, transform.position.z);
        }
    }
}