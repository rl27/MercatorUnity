using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    float moveSpeed = 0.01f;
    public Vector3d pos;
    public Vector3d dir;
    Vector3d right;

    CameraController cc;
    double sensitivity;
    double rotY;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pos = new Vector3d(0, 1, 0);
        dir = new Vector3d(0, 0, 1);
        right = new Vector3d(-1, 0, 0);

        cc = GameObject.FindObjectOfType<CameraController>();
        sensitivity = cc.sensitivity;
        rotY = cc.rotation.y;
    }

    // Update is called once per frame
    void Update()
    {
        // Camera rotation goes clockwise, not ccw
        float rotation = - Mathf.Deg2Rad * GameObject.Find("Main Camera").transform.eulerAngles.y;

        // Movement on the Euclidean xz plane
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        /*
        rb.velocity = new Vector3(Mathf.Cos(rotation) * horizontalInput - Mathf.Sin(rotation) * verticalInput,
                                  0,
                                  Mathf.Sin(rotation) * horizontalInput + Mathf.Cos(rotation) * verticalInput) * moveSpeed;
        */
         
        /***
        Direct XZ translation causes wonky movement when far from origin. New method:
        - Maintain player position at (0, 1, 0) and a tangent vector (i.e. direction).
        - Moving forward means moving in that direction (easy to get since position stays at (0, 1, 0)).
        - Keep position of closest tile on hyperboloid. When player moves, transform player back to (0, 1, 0)
          and transform the tile position along with it.
        ***/

        // Forward/backward; need to normalize or small errors will quickly build up
        double theta = - cc.rotation.y * Mathd.Deg2Rad + Mathd.PI_PRECISE / 2;
        dir = new Vector3d(Mathd.Cos(theta), 0, Mathd.Sin(theta));
        pos = Hyper.hypNormalize(Hyper.lineDir(pos, dir, moveSpeed * verticalInput));

        // Right/left
        theta = theta - Mathd.PI_PRECISE / 2;
        right = new Vector3d(Mathd.Cos(theta), 0, Mathd.Sin(theta));
        pos = Hyper.hypNormalize(Hyper.lineDir(pos, right, moveSpeed * horizontalInput));
    }
}
