using System;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
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

    public void SetCanServe()
    {
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
        CheckInput();
    }

    private void FixedUpdate()
    {
        _mallet.Move();
    }

    private void CheckInput()
    {
        int currentDirection = 0;

        if (_canServe)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                _canServe = false;
                DoServe?.Invoke();
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
            _mallet.SetDirection(_lastDirection);
        }
    }
}
