using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmojiAnimation : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(DeleteHelper());
	}

    IEnumerator DeleteHelper()
    {
        float time = Time.time;
        float amp = Random.Range(30.0f, 50.1f);
        amp *= Random.Range(0,2) *2-1;
        float randPeriod = Random.Range(0.0f, (Mathf.PI * 2));
        float spd = Random.Range(85.0f, 125.0f);
        float startX = transform.position.x;

        float initTime = Time.time; // On start, or something
        float startFadeTime = 3f;
        float fadeOutTime = 2f;
        while (Time.time - time < 5)
        {
            transform.position = new Vector3(startX + (Mathf.Sin(Time.time * randPeriod) * amp), transform.position.y + spd * Time.deltaTime);
            if(Time.time - time >= 3)
            {
                Image[] imgs = GetComponentsInChildren<Image>();
                for(int i =0; i < imgs.Length; i++)
                {
                    Color c = new Color();
                    c.b = imgs[i].color.b;
                    c.g = imgs[i].color.g;
                    c.r = imgs[i].color.r;
                    c.a = 1f - (Time.time - initTime - startFadeTime) / fadeOutTime;
                    imgs[i].color = c;
                }
            }
            yield return null;
        }

        Destroy(gameObject);
        yield return null;
    }
}
