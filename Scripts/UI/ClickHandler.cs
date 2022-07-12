using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ClickHandler : MonoBehaviour
{
    public List<UnityEvent> callbacks;

	public void OnMouseUp() {
		callbacks.ForEach((UnityEvent evt)=>{
            evt.Invoke();
        });
	}
}
