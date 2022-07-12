using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceSwitcher : MonoBehaviour {

    const float SPD = 8f;

    public GameObject switchButton;


    private bool onFirstSlide = true;
    Vector3 startPos;
    Vector3 endPos;

	// Use this for initialization
	void Start () {
        startPos = transform.localPosition;
        endPos = new Vector3(startPos.x + 1871, startPos.y, startPos.z);
	}

	// Update is called once per frame
	void Update () {
        transform.localPosition = Vector3.Lerp(transform.localPosition, onFirstSlide ? startPos : endPos, SPD * Time.deltaTime);
	}

    public void SwitchCards(bool goRight)
    {
        onFirstSlide = !onFirstSlide;
        switchButton.GetComponentInChildren<Text>().text = onFirstSlide ? "Show Development Cards" : "Show Resource Cards";
        switchButton.transform.Rotate(180f, 180f, 0f);
    }
}
