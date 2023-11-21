using System;
using UnityEngine;

public class AiPlayer : MonoBehaviour
{
    public Action DoServe;
    public int PlayerNumber { get => _playerNumber; }
    private Mallet _mallet;
    private bool _canServe;
    private const float SERVE_WAIT_SECS = 1.5f;              // The delay before serving after being given ownership of the ball
    private const float DIRECTION_CHANGE_DELAY_SECS = .12f;  // How long it takes for the ai player to "react" to a direction change
    private const float MOVE_THRESHOLD = .5f;                // Min target delta before moving
    private const float MIN_TRACKING_DISTANCE = 7f;          // Min distance along vertical axis before target tracking starts.
    private const float SPEED = .14f;                        // Max player moving speed
    private float _directionChangeWait = 0f;
    private float _serveWait = 0f;
    private int _lastDirection = 0;
    private int _playerNumber;
    private Transform _target;

    private void Awake()
    {
        _mallet = GetComponent<Mallet>();
        _mallet.Speed = SPEED;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void ResetToDefault()
    {
        _target = null;
        _canServe = true;
    }

    public void SetCanServe()
    {
        _serveWait = SERVE_WAIT_SECS;
        _canServe = true;
    }

    private void DoShow()
    {
        _mallet.Show();
    }

    public void Show(int playerNumber)
    {
        _playerNumber = playerNumber;
        DoShow();
    }

    private void Update()
    {
        FollowTarget();
    }

    private void FixedUpdate()
    {
        if (_canServe)
        {
            _serveWait -= Time.fixedDeltaTime;
            if (_serveWait <= 0f)
            {
                _canServe = false;
                DoServe?.Invoke();
            }
        }

        _mallet.Move();
    }

    private void FollowTarget()
    {
        if (_target == null)
        {
            return;
        }
        
        int currentDirection = 0;
        float deltaX = _target.position.x - transform.position.x;
        float deltaZ = _target.position.z - transform.position.z;

        if (Mathf.Abs(deltaX) > MOVE_THRESHOLD && Mathf.Abs(deltaZ) < MIN_TRACKING_DISTANCE)
        {
            if (deltaX > 0f)
            {
                currentDirection = 1;
            }
            else if (deltaX < 0f)
            {
                currentDirection = -1;
            }
        }

        if (currentDirection != _lastDirection)
        {
            if (_directionChangeWait == 0f)
            {
                _directionChangeWait = DIRECTION_CHANGE_DELAY_SECS;
                return;
            }
            else if (_directionChangeWait > 0f)
            {
                _directionChangeWait -= Time.deltaTime;
            }

            if (_directionChangeWait <= 0f)
            {
                _directionChangeWait = 0f;
                _lastDirection = currentDirection;
                _mallet.SetDirection(_lastDirection);
            }
        }
    }
}
