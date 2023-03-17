using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    // Default distance
    private float dist = 50f;
    private float temp = 50f;
    private GameObject optionsMenu;
    private bool active = false;
    private int projection = 0;

    Polygons polygons;
    
    void Start()
    {
        optionsMenu = GameObject.Find("OptionsMenu");
        optionsMenu.SetActive(false);

        polygons = GameObject.Find("TileSpawner").GetComponent<Polygons>();
        polygons.setDist(dist);
        polygons.setProjection(projection);
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
        polygons.setDist(dist);
    }

    public void inputDist(string input)
    {
        float temp2;
        if (float.TryParse(input, out temp2))
            temp = temp2;
    }

    public void inputProjection(int input)
    {
        projection = input;
        polygons.setProjection(projection);
    }
}
