using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    // Start is called before the first frame update

    private int activeScene = 0;
    void Start()
    {
        activeScene = SceneManager.GetActiveScene().buildIndex;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play()
	{
        SceneManager.LoadScene(activeScene + 1);

    }

    public void Quit()
	{
        Application.Quit();
    }

    public void MainMenu()
	{
        SceneManager.LoadScene(0);
    }

    public void Retry()
	{
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
