using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScroll : MonoBehaviour {

    public List<Sprite> tutorialImages;
    public Image tutorialImage;
    public GameObject blackBars;

    GameState startState;
    int currentImg = 0;

    public void ShowTutorial()
    {
        if (tutorialImages.Count == 0) return;

        tutorialImage.sprite = tutorialImages[0];
        tutorialImage.gameObject.SetActive(true);
        blackBars.SetActive(true);
        if (GameController.gc)
        {
            startState = GameController.gc.currentState;
            GameController.gc.currentState = GameState.tutorial;
        }
    }

    public void ChangeTutorialImage()
    {
        currentImg++;
        if (currentImg >= tutorialImages.Count)
        {
            currentImg = 0;
            tutorialImage.gameObject.SetActive(false);
            blackBars.SetActive(false);
            if (GameController.gc)
                GameController.gc.currentState = startState;
        }
        else tutorialImage.sprite = tutorialImages[currentImg];
    }
}
