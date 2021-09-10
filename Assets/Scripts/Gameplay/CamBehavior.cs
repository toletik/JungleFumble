using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CamBehavior : MonoBehaviour
{
    [SerializeField] GameObject tacticCam;
    [SerializeField] GameObject playCam;
	[SerializeField] GameObject fade;
	[SerializeField] Sprite matGoToPlaymode;
	[SerializeField] Sprite matGoToTactical;
	[SerializeField] float transitionSpeed = 2.0f;

	private bool goingToPlay = true;
	private bool canSwitch = true;
	public bool isInSwitch = false;
	//private bool playcam = false;
	[SerializeField] GameManager gameManager = null;

	private void Start()
	{
		fade.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
		
	}




	void FadeIn()
	{
		fade.GetComponent<Image>().canvasRenderer.SetAlpha(1);
		fade.GetComponent<Image>().CrossFadeAlpha(0, transitionSpeed, false);
		fade.SetActive(false);
		canSwitch = true;

		if(!goingToPlay)
			gameManager.StartPlayMode();

	}

	void FadeOut()
	{

		fade.SetActive(true);
		fade.GetComponent<Image>().canvasRenderer.SetAlpha(0);
		fade.GetComponent<Image>().CrossFadeAlpha(1, transitionSpeed, false);

	}

	public void Fade()
	{
		if (canSwitch)
		{
			canSwitch = false;
			
			FadeOut();


			if (goingToPlay)
				fade.GetComponent<Image>().sprite = matGoToTactical;
			else
				fade.GetComponent<Image>().sprite = matGoToPlaymode;


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
