using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMatOnMouseOver : MonoBehaviour
{

    [SerializeField] Material mat = null;
    private Material currentMat;
    private Material firstMat;
    // Start is called before the first frame update
    void Start()
    {
        firstMat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnMouseOver()
	{
        GetComponent<Renderer>().material = mat;
	}

	private void OnMouseExit()
	{
        GetComponent<Renderer>().material = firstMat;
	}
}
