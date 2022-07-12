using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable.Unity;

public class AudioPool : MonoBehaviour {

    public static AudioPool ap;

    List<SFX> availablePool = new List<SFX>();
    List<SFX> usedPool = new List<SFX>();

    Dictionary<string, SFX> activeLoops = new Dictionary<string, SFX>();

    static int poolAmount = 20;

    public static bool muteOneShots;

    void Start()
    {
        if (ap == null)
        {
            ap = this;
        }
        else Destroy(this.gameObject);
        if (PTGameManager.singleton.IS_TABLETOP)
        {
            FillPool();
        }
    }

    // Populate SFX object pool with <poolAmount>
    void FillPool()
    {
        for (int i = 0; i < poolAmount; i++)
        {
            SFX newSound = new GameObject("sfx" + i.ToString(), typeof(SFX)).GetComponent<SFX>();
            newSound.transform.SetParent(transform);
            newSound.gameObject.SetActive(false);

            availablePool.Add(newSound);
        }
    }

    // Play a single sound once, with the given settings
    public static AudioSource PlayOneShot(AudioClip clip, float volume = 1, float pitch = 1)
    {
        if (muteOneShots)
            return null;

        SFX p = ap.GetSFX();
        if (!p) return null;

        // Order of operations for using SFX objects
        p.Set(clip, volume, pitch);
        p.Activate();
        return p.GetComponent<AudioSource>();
    }

    // Multi-use function for looping tracks
    // Set <clipName> in order to have future references to the audio source
    // If <clipName> already exists, it will be smoothly replaced
    // <clip> can be left blank to modify the <clipName> SFX, regardless of what AudioClip it is playing
    public static SFX PlayLoop(AudioClip clip, string clipName, float startVolume, float endVolume, float duration, bool loop = true)
    {
        SFX p = null;
        bool exists = false;

        if (ap.activeLoops.ContainsKey(clipName))
        {
            p = ap.activeLoops[clipName]; // Set reference if clipName already exists

            if (p.aud.clip.Equals(clip) || !clip) // If !clip, adjust volume of existing audio
            {
                if (p.aud.volume == endVolume) return null;
                exists = true;
                if (p.aud.volume != startVolume) startVolume = p.aud.volume;
            }
            else
            {
                // Same clipName, different clip playing. Toggles a simultaneous fade in/out, using an additional SFX.
                p.SetFade(p.aud.volume, 0f, duration);
                p.Activate();
                ap.activeLoops.Remove(clipName);
                // Removes fading out SFX from dictionary, treat new clip as a new SFX
            }
        }
        
        if (!exists) // If there is no existing clip, or if there is a transition
        {
            p = ap.GetSFX();
            if (!p) return null;
        }

        if (!exists)
        {
            ap.activeLoops.Add(clipName, p); // Add to dictionary
            if (clip) p.Set(clip, startVolume, loop); // If !clip, clip and volume do not need to be set
        }

        p.SetFade(startVolume, endVolume, duration);
        p.Activate();
        return p;
    }

    public static void StopAllSounds(float fadeTime, string[] dontStop = null)
    {
        if (ap == null)
            return;

        foreach(SFX s in ap.usedPool)
        {
            bool cont = false;
            if(dontStop != null) foreach (string str in dontStop) if (ap.activeLoops.ContainsKey(str)) cont = true;
            if (cont) continue;
            s.SetFade(s.aud.volume, 0, fadeTime);
            s.Activate();
        }
    }

    // Used for SFX objects to reset and re-add themselves to the pool
    public static void Reset(SFX a)
    {
        a.Set(null, 1, 1);
        a.Deactivate();
        ap.usedPool.Remove(a);

        if (ap.activeLoops.ContainsValue(a))
        {
            foreach (KeyValuePair<string, SFX> o in ap.activeLoops)
            {
                if (o.Value.Equals(a))
                {
                    ap.activeLoops.Remove(o.Key);
                    break;
                }
            }
        }

        ap.availablePool.Add(a);
        a.gameObject.SetActive(false);
    }

    SFX GetSFX()
    {
        if (ap.availablePool.Count < 1)
        {
            Debug.LogError("Pool is empty!"); // Log error if pool is empty
            return null;
        }
        ap.availablePool[0].gameObject.SetActive(true); // Activate first pooled object and remove from list
        SFX p = ap.availablePool[0];
        ap.availablePool.RemoveAt(0);
        ap.usedPool.Add(p);

        return p;
    }
}
