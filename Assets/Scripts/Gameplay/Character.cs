using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;


public class Character : MonoBehaviour
{
    [SerializeField] public int mvt;
    [SerializeField] public int strength = 0;
    [SerializeField] public int range = 0;


    [SerializeField] public Transform charactePlaymode = null;

    [SerializeField] public GameObject ballIcon = null;
    public bool hasBall = false;
    public bool canPickUpBall = true;

    [SerializeField] public GameObject characterCard        = null;
    [SerializeField] public Texture    characterCardTexture = null;


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


}
