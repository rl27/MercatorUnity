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
    PlayerController playerController;
    CameraController cameraController;

    void Start()
    {
        optionsMenu = GameObject.Find("OptionsMenu");
        optionsMenu.SetActive(false);

        polygons = GameObject.Find("TileSpawner").GetComponent<Polygons>();
        polygons.setDist(dist);
        polygons.setProjection(projection);

        cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            toggleGame(!active);
        }
    }

    public void toggleGame(bool toggle)
    {
        active = toggle;
        optionsMenu.SetActive(toggle);
        playerController.disableMovement = toggle;
        cameraController.disableMovement = toggle;   
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
