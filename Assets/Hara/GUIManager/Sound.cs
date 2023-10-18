using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundsCollection", menuName = "SoundsCollection")]
public class Sound : ScriptableObject
{
    private static Sound instance;
    
    public static Sound Instance
    {
        get
        {
            if (instance == null) instance = Resources.Load<Sound>("SoundsCollection");
            return instance;
        }
    }
    
    public AudioClip[] clips;

    public AudioClip GetClip(string clipName)
    {
        if (clipName == null || clips.Length == 0)
        {
            return null;
        }

        return System.Array.Find(clips, e => e != null && e.name == clipName);
    }
}