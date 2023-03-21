using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3d rotation = new Vector3d(0, 0, 0);
    public double sensitivity = 0.01;
    public bool disableMovement = false;

    private float DEFAULT_HEIGHT = 0.3f;
    private float MIN_HEIGHT = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(transform.position.x, DEFAULT_HEIGHT, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (disableMovement) return;
        
        rotation.y += sensitivity * Input.GetAxis("Mouse X");
        rotation.x -= sensitivity * Input.GetAxis("Mouse Y");
        rotation.x = Mathd.Clamp(rotation.x, -90d, 90d);
        
        transform.eulerAngles = (Vector3) rotation;

        // transform.position = GameObject.Find("Player").transform.position;

        // Camera zoom via scroll wheel
        // Camera.main.fieldOfView -= Input.mouseScrollDelta.y;
        
        // Height change via scroll wheel
        float newY = Mathf.Max(MIN_HEIGHT, transform.position.y - 0.05f * Input.mouseScrollDelta.y);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
