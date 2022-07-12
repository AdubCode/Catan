using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour {

    static string[] Tutorial_Text = new string[] {
        "First place a settlment by clicking an open edge of a tile.",
        "Now that you have placed a settlment you can build a road going in any direction from that settlment.",
        "There is nothing else that you can do so you can end your turn by hitting end turn",
        "Now you must place a second settlment somewhere on the board.",
        "Now its time to place a second road in any direction from the settlement that you just placed.",
        "To win a player needs 10 victory points. You can earn a victory point for each Settlement you have, you can earn 2 victory points for each City you have.",
        "You have to many cards you must remove some",
        "Everyone has removed enough cards you may move the robber",
    };

    public static void ShowText(int textNum, Player player)
    {
        if (player.usingTutorial && !player.isAI)
        {
            //Debug.Log("Tutorial Text " + Tutorial_Text[textNum]);
        }else
        {
            //make it so that tutorial text doesn't show anything
        }
    }
}
