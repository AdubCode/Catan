using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffects : MonoBehaviour {

    public List<AudioClip> buildCity;
    public List<AudioClip> buildRoad;
    public List<AudioClip> buildSettlement;

    public List<AudioClip> getBricks;
    public List<AudioClip> getGrain;
    public List<AudioClip> getSheep;
    public List<AudioClip> getStone;
    public List<AudioClip> getWood;

    public List<AudioClip> knight;
    public List<AudioClip> robber;

    public List<AudioClip> longLoops;
    public List<AudioClip> music;



    public static Dictionary<string, List<AudioClip>> soundEffects = new Dictionary<string, List<AudioClip>>();

    void Awake()
    {
        soundEffects.Add("Build City", buildCity);
        soundEffects.Add("Build Road", buildRoad);
        soundEffects.Add("Build Settlement", buildSettlement);

        soundEffects.Add("Get Bricks", getBricks);
        soundEffects.Add("Get Grain", getGrain);
        soundEffects.Add("Get Sheep", getSheep);
        soundEffects.Add("Get Stone", getStone);
        soundEffects.Add("Get Wood", getWood);

        soundEffects.Add("Knight", knight);
        soundEffects.Add("Robber", robber);

        soundEffects.Add("Long Loops", longLoops);
        soundEffects.Add("Music", music);
    }
}
