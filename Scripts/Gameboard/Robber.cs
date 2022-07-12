using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robber : MonoBehaviour {

    [HideInInspector] public bool needsToMove {
        get { return _needsMoving; }
        set {
            _needsMoving = value;
            // Side effects in a setter, what could go wrong?!
            EnableGlow(value);
        }
    }

    private bool _needsMoving;

    GameObject currentTile;
    bool followMouse;

    Material glowMat;
    Material normalMat;
    MeshRenderer rend;

    void Awake() {
        rend = gameObject.GetComponent<MeshRenderer>();

        normalMat = new Material(rend.material);
        glowMat =  Resources.Load("GlowPulse") as Material;
    }

    void EnableGlow(bool yesno) {
        rend.material = yesno ? glowMat : normalMat;
    }

	// Update is called once per frame
	void Update () {
        if (followMouse)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.transform.position.y - transform.lossyScale.y;

            Vector3 pos = Camera.main.ScreenToWorldPoint(mousePosition);
            pos.y = 0.3f;
            transform.position = Vector3.Lerp(transform.position, pos, 0.5f);
        } else {
            transform.position = Vector3.Lerp(transform.position, (currentTile.transform.position + (Vector3.forward * .5f)), Time.deltaTime * 5f);
        }
	}

    public void MoveToTile(GameObject destination) {
        if(destination != currentTile)
        {
            SetCurrentTile(destination);
            int rand = Random.Range(0, SoundEffects.soundEffects["Robber"].Count);
            AudioPool.PlayLoop(SoundEffects.soundEffects["Robber"][rand], "Robber", 1, 1, SoundEffects.soundEffects["Robber"][rand].length, false);
            needsToMove = false;
            followMouse = false;

            // AI characters are not shown the "pick a card" screen.
            if (!GameController.gc.GetCurrentPlayer().isAI){
                GameController.gc.DisplayStealableResources(destination.GetComponent<Tile>());
            }
        }
    }

    public void OnMouseUp()
    {
        GameObject g = currentTile;
        for(int i = 0; i < GameController.gc.boardsTiles.Count; i++)
        {
            //finds closest tiles center
            if ((transform.position - g.transform.position).magnitude > (transform.position - GameController.gc.boardsTiles[i].transform.position).magnitude) g = GameController.gc.boardsTiles[i].gameObject;
        }

        MoveToTile(g);
    }

    void OnMouseDown()
    {
        if (!needsToMove || GameController.gc.TooManyCards()) return;
        followMouse = true;
    }

    public void SetCurrentTile(GameObject go)
    {
        if(currentTile == null) {
            // If no tile exists already, it's the first placement of the game, so just snap it into position.
            transform.position = new Vector3(go.transform.position.x, transform.position.y, go.transform.position.z + .5f);
        } else {
            currentTile.GetComponent<Tile>().hasRobber = false;
        }
        currentTile = go;
        currentTile.GetComponent<Tile>().hasRobber = true;
    }
}
