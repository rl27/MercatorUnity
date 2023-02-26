using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    float moveSpeed = 0.003f;
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

        /*
        Vector3d p = new Vector3d(1, Mathd.Sqrt(6), 2);
        Vector3d q = new Vector3d(2, Mathd.Sqrt(14), 3);
        Vector3d d = Hyper.getDir(p, q);
        Vector3d r = Hyper.rotateAxis(d, Hyper.gradient(p), Mathd.PI_PRECISE / 2);
        
        Vector3d d2 = Hyper.getDir(q, p);
        Vector3d r2 = Hyper.rotateAxis(d2, Hyper.gradient(q), Mathd.PI_PRECISE / 2);

        Vector3d newP = Hyper.lineDir(p, r, 0.3);
        Vector3d newQ = Hyper.lineDir(q, r2, -0.3);
        Vector3d newD = Hyper.getDir(newP, newQ);
        Debug.Log(newP);
        Debug.Log(newQ);
        Debug.Log(newD);

        Vector3d s = Hyper.lineDir(p, d, 0.7);
        Debug.Log(s);
        */
        
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
        Direct XZ translation causes wonky movement when far from origin? New method:
        - Maintain a position on the hyperboloid and a tangent vector (i.e. direction).
        - Moving forward means moving in that direction.
        - Looking around means rotating the tangent vector around in the plane tangent to the hyperboloid at the position.
        ***/

        // Gradient gives normal to hyperboloid
        Vector3d norm = new Vector3d(2 * pos.x, -2 * pos.y, 2 * pos.z);

        // Forward/backward
        pos = Hyper.lineDir(pos, dir, moveSpeed * verticalInput);

        // Right/left
        /*
        right = Hyper.rotateAxis(dir, norm, Mathd.PI_PRECISE / 2);
        pos = Hyper.lineDir(pos, right, horizontalInput);
        dir = ???????
        */

        
        // double theta = - sensitivity * Input.GetAxis("Mouse X") * Mathd.Deg2Rad;
        double theta = (- cc.rotation.y + rotY) * Mathd.Deg2Rad;
        rotY = cc.rotation.y;
        dir = Hyper.rotateAxis(dir, norm, theta);
    }
}
