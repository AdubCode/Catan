using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomResourceUI : MonoBehaviour {

    GameController gc;
    PlayerColors color;

    public void SetUp(PlayerColors c, GameController gameController)
    {
        gc = gameController;
        color = c;
    }

    public void ButtonClicked()
    {
        gc.TookResource(color);
    }
}
