using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{

    public Animator transitionAnimation;
    public string nextSceneName;

    public List<UnityEvent> callbacks;


    public void StartTransition()
    {
        StartCoroutine("LoadScene");
    }

    IEnumerator LoadScene()
    {
        transitionAnimation.SetTrigger("Exit");
        yield return new WaitForSeconds(2f);

        callbacks.ForEach((UnityEvent evt)=>{
            evt.Invoke();
        });
    }
}
