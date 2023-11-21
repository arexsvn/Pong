using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalGameplayManager
{
    public Action<int> playerScored;
    public Action<List<string>> gameStarted;
    public Action gameRestarted;
    private LocalBall _ball;
    private List<Vector3> _startPositions = new List<Vector3> { new Vector3(0, 0, 9), new Vector3(0, 0, -9) };
    private string BALL_PREFAB = "Gameplay/LocalBall";
    private string LOCAL_PLAYER_PREFAB = "Gameplay/LocalPlayer";
    private string AI_PLAYER_PREFAB = "Gameplay/AiPlayer";
    private LocalPlayer _localPlayer;
    private AiPlayer _aiPlayer; 

    public LocalGameplayManager()
    {

    }

    public void Init()
    {

    }

    public void Start()
    {
        _localPlayer = UnityEngine.Object.Instantiate(Resources.Load<LocalPlayer>(LOCAL_PLAYER_PREFAB), _startPositions[1], Quaternion.identity);
        _localPlayer.DoServe += handlePlayerServe;

        _aiPlayer = UnityEngine.Object.Instantiate(Resources.Load<AiPlayer>(AI_PLAYER_PREFAB), _startPositions[0], Quaternion.identity);
        _aiPlayer.DoServe += handlePlayerServe;


        _localPlayer.Show(1);
        _aiPlayer.Show(0);

        createBall(_localPlayer, _aiPlayer);

        gameStarted?.Invoke(new List<string> { "Player", "AI" });
    }

    public void RestartGame()
    {
        createBall(_localPlayer, _aiPlayer);

        gameRestarted?.Invoke();
    }

    public void Cleanup()
    {
        EndGame();

        if (_localPlayer != null)
        {
            _localPlayer.DoServe -= handlePlayerServe;
            GameObject.Destroy(_localPlayer.gameObject);
            _localPlayer = null;
        }

        if (_aiPlayer != null)
        {
            _aiPlayer.DoServe -= handlePlayerServe;
            GameObject.Destroy(_aiPlayer.gameObject);
            _aiPlayer = null;
        }
    }

    public void EndGame()
    {
        if (_aiPlayer != null)
        {
            _aiPlayer.ResetToDefault();
        }
        
        if (_ball != null)
        {
            _ball.ClearOwner();
            _ball.OutOfBounds -= handleOutOfBounds;
            GameObject.Destroy(_ball.gameObject);  
            _ball = null;
        }
    }

    private void handlePlayerServe()
    {
        // Check for null in case the aiplayer tries to server after gameover.
        if (_ball == null)
        {
            return;
        }
        
        int direction = 1;
        if (_ball.Owner.position.z > 0)
        {
            direction = -1;
        }

        _ball.ClearOwner();
        _ball.SetRandomVelocity(direction);
    }

    private void createBall(LocalPlayer localPlayer, AiPlayer aiPlayer)
    {
        Transform playerTransform = localPlayer.transform;
        
        _ball = UnityEngine.Object.Instantiate(Resources.Load<LocalBall>(BALL_PREFAB), playerTransform.position, Quaternion.identity);
        _ball.OutOfBounds += handleOutOfBounds;
        _ball.Owner = playerTransform;

        aiPlayer.SetTarget(_ball.transform);

        localPlayer.SetCanServe();
    }

    private void handleOutOfBounds()
    {
        _ball.ResetToStart();

        int playerNumber = 0;

        // Player who did not score takes posession in this version of the rules.
        if (_ball.transform.position.z > 0)
        {
            playerNumber = 1;
            _ball.Owner = _aiPlayer.transform;
            _aiPlayer.SetCanServe();
        }
        else
        {
            _ball.Owner = _localPlayer.transform;
            _localPlayer.SetCanServe();
        }

        _ball.transform.position = _ball.Owner.position;

        playerScored?.Invoke(playerNumber);
    }
}