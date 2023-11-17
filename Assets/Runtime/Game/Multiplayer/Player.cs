using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    private NetworkTransform _networkTransform;
    private Renderer _renderer;
    private Vector2 _bounds = new Vector2(-4, 4);
    private float _speed = .26f;
    private int _direction = 0;
    private int _lastDirection = 0;

    private void Awake()
    {
        _networkTransform = GetComponent<NetworkTransform>();
        _renderer = GetComponent<Renderer>();
        _renderer.enabled = false;
    }

    /*
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            //Move();
        }
    }
    */

    [ClientRpc]
    public void ShowClientRpc()
    {
        DoShow();
    }

    private void DoShow()
    {
        _renderer.enabled = true;
    }

    public void Show()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            ShowClientRpc();

            // Show on dedicated server as well
            if (!NetworkManager.Singleton.IsHost) 
            {
                DoShow();
            }
        }
    }
    /*
    public void Move()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }
        else
        {
            SubmitPositionRequestServerRpc();
        }
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = GetRandomPositionOnPlane();
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(UnityEngine.Random.Range(-3f, 3f), 1f, UnityEngine.Random.Range(-3f, 3f));
    }
    */
    private void Update()
    {
        //transform.position = Position.Value;
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
    void SubmitMoveRequestServerRpc(int direction)
    {
        _direction = direction;
    }

    void Move()
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