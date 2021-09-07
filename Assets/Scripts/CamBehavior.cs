using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CameraKind
{
    E_TACTICCAM = 0,
    E_PLAYCAM = 1,

}

public class CamBehavior : MonoBehaviour
{

    [SerializeField] Transform[] views;
    [SerializeField] float transitionTime;

  

    Transform currentView = null;
    int camNumber = 0;

    // Start is called before the first frame update
    void Start()
    {
        currentView = views[0];
    }


	private void Update()
	{
        if (Input.GetKeyDown(KeyCode.A))
            currentView = views[0];
        if (Input.GetKeyDown(KeyCode.Z))
            currentView = views[1];
    }

	public void switchView(CameraKind cameraKind)
	{
        currentView = views[(int)cameraKind];
        
    }

	void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, currentView.position, transitionTime * Time.deltaTime);
    }
}
