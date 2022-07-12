using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayTable.Unity;

public class StartSound : MonoBehaviour {

    bool called = false;

    void Update()
    {
        if (AudioPool.ap != null &&  !called)
        {
            int rand = Random.Range(0, SoundEffects.soundEffects["Long Loops"].Count);
            AudioPool.PlayLoop(SoundEffects.soundEffects["Long Loops"][rand], "Long Loops", .45f, .45f, SoundEffects.soundEffects["Long Loops"][rand].length);

            AudioPool.PlayLoop(SoundEffects.soundEffects["Music"][0], "Music", .6f, .6f, SoundEffects.soundEffects["Music"][0].length);
            called = true;
        }
    }
}
