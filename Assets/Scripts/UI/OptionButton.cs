using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionButton : MonoBehaviour
{
    [SerializeField] GameObject optionMenu = null;
    [SerializeField] GameObject mainMenu = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
 
    }

    private void OnMouseDown()
    {
        optionMenu.SetActive(true);
        mainMenu.SetActive(false);
    }
}
