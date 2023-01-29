using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    float moveSpeed = 2f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Camera rotation goes clockwise, not ccw
        float rotation = - Mathf.Deg2Rad * GameObject.Find("Main Camera").transform.eulerAngles.y;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        rb.velocity = new Vector3(Mathf.Cos(rotation) * horizontalInput - Mathf.Sin(rotation) * verticalInput,
                                  0,
                                  Mathf.Sin(rotation) * horizontalInput + Mathf.Cos(rotation) * verticalInput) * moveSpeed;
    }
}
