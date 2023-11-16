using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController
{
    private System.Action _exit;
    readonly LevelController _levelController;
    public SceneController(LevelController levelController)
    {
        _levelController = levelController;
    }

    public void init(System.Action exit)
    {
        _exit = exit;
        _levelController.init(exit);
    }

    public void start()
    {
        _levelController.start();
    }

    private void exit()
    {
        _exit();
    }
}
