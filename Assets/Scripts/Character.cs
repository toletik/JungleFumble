using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
struct Stats
{
   float mvt;
   float strength;
   float range;
   float precision;
   float catchUp;
}*/

public class Character : MonoBehaviour
{
    [SerializeField] public int mvt;
    [SerializeField] public int strength = 0;
    [SerializeField] public int range = 0;
    [SerializeField] public int precision = 0;
    [SerializeField] public int catchUp = 0;

    public List<int> queueTileIndex = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
