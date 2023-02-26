using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3d rotation = new Vector3d(0, 0, 0);
    public double sensitivity = 0.01;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        rotation.y += sensitivity * Input.GetAxis("Mouse X");
        rotation.x -= sensitivity * Input.GetAxis("Mouse Y");
        rotation.x = Mathd.Clamp(rotation.x, -90d, 90d);
        
        transform.eulerAngles = (Vector3) rotation;

        transform.position = GameObject.Find("Player").transform.position;
    }
}
