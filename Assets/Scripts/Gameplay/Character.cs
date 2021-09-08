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

    [SerializeField] public GameObject ballIcon = null;
    public bool hasBall = false;
    public bool canPickUpBall = true;

    public Vector3 initialPos;
    public List<int> queueTileIndex = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        initialPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball") && canPickUpBall)
        {
            hasBall = true;
            ballIcon.SetActive(true);
            other.GetComponent<LineRenderer>().positionCount = 0;
            other.gameObject.SetActive(false);
        }
    }

}