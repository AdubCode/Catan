using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour {

	Vector3 originalPosition;
	public Vector3 direction = Vector3.forward;
	float timeOffset;

	void Start () {
		originalPosition = transform.localPosition;
		timeOffset = Random.value * 2f;
	}

	void Update () {
		transform.localPosition = originalPosition + (Mathf.Sin(Time.time + timeOffset) * (direction) * 0.66f);
	}
}
