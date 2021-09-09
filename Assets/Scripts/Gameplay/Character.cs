using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

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


    [SerializeField] public Transform charactePlaymode = null;
    [SerializeField] public GameObject ballIcon = null;
    public bool hasBall = false;
    public bool canPickUpBall = true;
    [SerializeField] GameObject characterCard = null;
    [SerializeField] float timeToShowCard = 1;
    float currentTimeToShowCard = 0;

    public Vector3 initialPos;
    public List<int> queueTileIndex = new List<int>();

    [SerializeField] public string blocSound = "";
    [SerializeField] public string catchSound = "";
    [SerializeField] public string cardSound = "";

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
            RuntimeManager.PlayOneShot(catchSound);
        }
    }


    private void OnMouseOver()
    {
        currentTimeToShowCard += Time.deltaTime;

        if(currentTimeToShowCard >= timeToShowCard)
            characterCard.SetActive(true);
    }

    private void OnMouseExit()
    {
        currentTimeToShowCard = 0;
        characterCard.SetActive(false);
    }
}
