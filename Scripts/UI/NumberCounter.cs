using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NumberCounter : MonoBehaviour {

	private float targetNumber = 0f;
	private float currentNumber = 0f;

	private TextMeshPro tmpLabel;
	private Text label;

	private void Start() {
		tmpLabel = GetComponentInChildren<TextMeshPro>();
		if(tmpLabel == null) {
			label = GetComponentInChildren<Text>();
		}
	}

	public void SetNumber(float value){
		targetNumber = value;
	}

	public float GetNumber() {
		return targetNumber;
	}

	public float GetDisplayedNumber() {
		return currentNumber;
	}

	// Update is called once per frame
	void Update () {
		currentNumber = Mathf.Lerp(currentNumber, targetNumber, Time.deltaTime * 4f);
		if (tmpLabel != null) {
			tmpLabel.text = Mathf.RoundToInt(currentNumber).ToString();
		} else {
			label.text = Mathf.RoundToInt(currentNumber).ToString();
		}
	}
}
