using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EmojiClicked : MonoBehaviour, IPointerDownHandler{

    public void OnPointerDown(PointerEventData e)
    {
        GameObject go = Instantiate((GameObject)Resources.Load("Emoji"));

        go.transform.SetParent(GameObject.Find("Canvas").transform);
        go.transform.position = transform.position;
        go.transform.localScale = new Vector3(.75f,.75f,.75f);
        go.GetComponentInChildren<Image>().sprite = GetComponent<Image>().sprite;
    }
}
