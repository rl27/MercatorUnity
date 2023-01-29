using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = GameObject.Find("Player").transform.position;

        //if (Input.GetKey(KeyCode.Space))
        //    rb.velocity = new Vector3(rb.velocity.x, 5f, rb.velocity.z);
    }
}
