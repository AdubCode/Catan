using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayedResourceCardEffect : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(RunCard());
	}

    IEnumerator RunCard()
    {
        GameController.gc.animationPlaying = true;
        yield return new WaitForSeconds(1);
        gameObject.AddComponent<Float>();
        yield return new WaitForSeconds(2);
        gameObject.GetComponent<Animator>().Play("CardDestroyed");
        gameObject.GetComponent<Float>().enabled = false;
        yield return new WaitForSeconds(1);
        GameController.gc.animationPlaying = false;
        Destroy(gameObject);
    }
}
