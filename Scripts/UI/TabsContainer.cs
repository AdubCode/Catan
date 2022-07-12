using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabsContainer : MonoBehaviour {
	private int currentTabIndex = 1;

	public List<Transform> tabs;
	
	public void ChangeTab(int index) {
		Debug.Assert(index > 0 && index < 4, "Tab index should be between 1, 2, or 3");
		if (index == currentTabIndex) {
			return;
		}

		Transform outgoingTab = transform.Find("Tab" + currentTabIndex);
		Transform incomingTab = transform.Find("Tab" + index);

		outgoingTab.Find("Content").gameObject.SetActive(false);
		incomingTab.Find("Content").gameObject.SetActive(true);

		Vector3 incomingPosition = outgoingTab.position;
		Vector3 outgoingPosition = incomingTab.position;

		incomingTab.position = incomingPosition;
		outgoingTab.position = outgoingPosition;

		currentTabIndex = index;
	}
}
