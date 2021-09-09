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
        currentMat = GetComponent<Renderer>().material;
        firstMat = currentMat;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnMouseOver()
	{
        currentMat = mat;
        Debug.Log("onMouseOver");
	}

	private void OnMouseExit()
	{
        currentMat = firstMat;
	}
}
