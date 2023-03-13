using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    private float dist = 50;
    private float temp = 50;
    private GameObject optionsMenu;
    private bool active = false;
    
    void Start()
    {
        optionsMenu = GameObject.Find("OptionsMenu");
        optionsMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            if (!active) {
                active = true;
                optionsMenu.SetActive(true);
            }
            else ResumeGame();
        }
    }

    public void ResumeGame()
    {
        active = false;
        optionsMenu.SetActive(false);
    }

    public void setDist()
    {
        dist = temp;
    }

    public void inputDist(string input)
    {
        float temp2;
        if (float.TryParse(input, out temp2))
            temp = temp2;
    }

    public float getDist()
    {
        return dist;
    }
}
