using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3d rotation = Vector3d.zero;
    double sensitivity = 3;

    // Start is called before the first frame update
    void Start()
    {
        Vector3d v = new Vector3d(1d, 4d, 2d);
        Debug.Log(Hyper.hypEval(v));
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
