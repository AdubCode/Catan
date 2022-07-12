using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTradeDrag : MonoBehaviour {
	public Player player;
	public ResourceType type;

	private bool isDragging = false;
	private bool hasLeftArea = false;

	void OnMouseDown() {
		GameController gc = GameController.gc;
		if (!player.HasResource(type, 1) || gc.currentState == GameState.setup || !gc.playerRolled || isDragging) {
			return;
		}
		gc.SetResourceInHand(type, gc.GetPlayerNumber(player));

		isDragging = true;
		hasLeftArea = false;
	}

	void Update() {
		if (isDragging && Input.GetMouseButtonUp(0))
        {
			isDragging = false;
			hasLeftArea = false;
        }
	}

	void OnMouseExit() {
		if (!isDragging || hasLeftArea) return;
		hasLeftArea = true;
		
		GameObject newCard = Instantiate(gameObject);
		newCard.transform.rotation = gameObject.transform.rotation;
		foreach (Transform child in newCard.transform) {
			GameObject.Destroy(child.gameObject);
		}
		newCard.layer = LayerMask.NameToLayer("Temporary");

		FollowMouse mouse = newCard.AddComponent<FollowMouse>();
		newCard.name = "DraggedCard";
		mouse.startPosition = transform.position;

		newCard.transform.position = transform.position;
		newCard.transform.localScale = transform.localScale * 0.05f;
		mouse.trackingObject = newCard;
	}
}
