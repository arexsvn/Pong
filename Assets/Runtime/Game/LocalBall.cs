using System;
using UnityEngine;

public class LocalBall : MonoBehaviour
{
    public Action OutOfBounds;
    private Ball _ball;
    public Transform Owner { get => _ball.Owner; set => _ball.Owner = value; }

    private void Awake()
    {
        _ball = GetComponent<Ball>();
    }

    private void FixedUpdate()
    {
        _ball.Move();

        if (_ball.OutOfBounds)
        {
            OutOfBounds?.Invoke();
        }
    }

    public void ClearOwner()
    {
        _ball.Owner = null;
    }

    public void ResetToStart()
    {
        _ball.Stop();
    }

    public void SetRandomVelocity(int direction = 0)
    {
        _ball.SetRandomVelocity(direction);
    }

    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        _ball.HandleCollision(other.gameObject.transform);
    }
}