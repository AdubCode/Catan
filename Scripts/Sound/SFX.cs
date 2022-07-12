using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFX : MonoBehaviour { // Individual audio object for pool

    public AudioSource aud;

    float startTime;
    float startVol;
    float endVol;
    float duration = -1f;

    bool active;

	void Awake () {

        DontDestroyOnLoad(this);

        aud = GetComponent<AudioSource>(); // Set references
        aud.playOnAwake = false;
        active = false;
	}

    // First function call. Sets all values
    public void Set(AudioClip clip, float volume = 1f, bool loop = false, float pitch = 1f)
    {
        aud.clip = clip;
        aud.volume = volume;
        aud.pitch = pitch;
        aud.loop = loop;
    }

    // Override for Set (No loop)
    public void Set(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        Set(clip, volume, false, pitch);
    }

    public void SetFade(float start, float end, float t)
    {
        startVol = start;
        endVol = end;
        duration = t;
        startTime = Time.time;
        StopAllCoroutines();
    }

    // Activate the object
    public void Activate()
    {
        if (!aud.isPlaying) aud.Play();
        if (duration != -1f) StartCoroutine(FadeLoop());
        active = true;
    }

    // Deactivate the object
    public void Deactivate()
    {
        aud.Stop();
        active = false;
    }

    void Update() // Checks for end of clip, then deactivates
    {
        if (!active) return;

        if (!aud.isPlaying && (!aud.loop || aud.volume == 0)) AudioPool.Reset(this);
    }

    IEnumerator FadeLoop()
    {
        while (Time.time - startTime < duration)
        {
            aud.volume = startVol + ((Time.time - startTime) / duration) * (endVol - startVol);
            yield return null;
        }

        aud.volume = endVol;
        duration = -1; // Used to check if this SFX should be fading sound
        if (endVol == 0) aud.Stop();

        yield break;
    }
}
