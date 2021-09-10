using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeTime : MonoBehaviour
{

    [SerializeField] float maxLifeTime = 1;
    [SerializeField] float currentLifeTime = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentLifeTime -= Time.deltaTime;

        if(currentLifeTime <= 0)
        {
            currentLifeTime = maxLifeTime;
            gameObject.SetActive(false);
        }



    }
}
