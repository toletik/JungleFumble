using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CamBehavior : MonoBehaviour
{
    [SerializeField] GameObject tacticCam;
    [SerializeField] GameObject playCam;
	[SerializeField] Image fade;
	[SerializeField] float transitionSpeed = 2.0f;

	private bool goingToPlay = true;
	private bool canSwitch = true;
	//private bool playcam = false;

	private void Start()
	{
		fade.canvasRenderer.SetAlpha(0.0f);
		

	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
			Fade();
	}



	void FadeIn()
	{
		fade.CrossFadeAlpha(0, transitionSpeed, false);
		canSwitch = true;
	}

	void FadeOut()
	{
		fade.CrossFadeAlpha(1, transitionSpeed, false);
		
	}

	public void Fade()
	{
		if (canSwitch)
		{
			canSwitch = false;
			FadeOut();
			Invoke("SwitchCamera", transitionSpeed);
			Invoke("FadeIn", transitionSpeed);
		}
		
		
	}

	void SwitchCamera()
	{
		if (goingToPlay)
		{
			tacticCam.SetActive(false);
			playCam.SetActive(true);
			goingToPlay = false;
		}
		else
		{

			playCam.SetActive(false);
			tacticCam.SetActive(true);
			goingToPlay = true;
		}
	}

}
