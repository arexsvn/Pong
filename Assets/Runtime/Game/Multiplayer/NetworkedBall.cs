using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class NetworkedBall : NetworkBehaviour
{
    public Action OutOfBounds;
    private Ball _ball;

    private void Awake()
    {
        _ball = GetComponent<Ball>();
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            _ball.Move();

            if (_ball.OutOfBounds)
            {
                OutOfBoundsClientRpc();

                if (!NetworkManager.Singleton.IsHost)
                {
                    handleOutOfBounds();
                }
            }
        }
    }

    public void ClearOwner()
    {
        _ball.Owner = null;
    }

    public void SetOwner(NetworkTransform owner)
    {
        _ball.Owner = owner.transform;
    }

    public Transform GetOwner() 
    {
        return _ball.Owner;
    }

    public void ResetToStart()
    {
        _ball.ResetToStart();
    }

    public void SetRandomVelocity(int direction = 0)
    {
        _ball.SetRandomVelocity(direction);
    }

    [ClientRpc]
    public void OutOfBoundsClientRpc()
    {
        handleOutOfBounds();
    }

    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
        if (!IsServer)
        {
            return;
        }

        _ball.HandleCollision(collision);
    }

    private void handleOutOfBounds()
    {
        OutOfBounds?.Invoke();
    }
}