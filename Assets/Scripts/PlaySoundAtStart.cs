using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class PlaySoundAtStart : MonoBehaviour
{
    [SerializeField] List<string> soundsToPlay = new List<string>();


    // Start is called before the first frame update
    void Start()
    {
        foreach(string sound in soundsToPlay)
            RuntimeManager.PlayOneShot(sound);        
    }
    
}
