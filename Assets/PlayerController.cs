using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    float moveSpeed = 0.02f;
    public Vector3d pos;
    public Vector3d dir;
    Vector3d right;

    public bool disableMovement = false;

    CameraController cc;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pos = new Vector3d(0, 1, 0);
        dir = new Vector3d(0, 0, 1);
        right = new Vector3d(-1, 0, 0);

        cc = GameObject.FindObjectOfType<CameraController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (disableMovement) return;
        
        // Camera rotation goes clockwise, not ccw
        float rotation = - Mathf.Deg2Rad * GameObject.Find("Main Camera").transform.eulerAngles.y;

        // Movement on the Euclidean xz plane
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
         
        /***
        Movement method:
        - Maintain player position at (0, 1, 0) and a tangent vector (i.e. direction).
        - Moving forward means moving in that direction (easy to get since position stays at (0, 1, 0)).
        - Keep position of closest tile on hyperboloid. When player moves, transform player back to (0, 1, 0)
          and transform the tile position along with it.
        - Constantly set angle of current tile so the vertex positions match.
        ***/

        // Alter move speed based on frame rate
        float ms = moveSpeed * (60f / FrameRate.GetCurrentFPS());

        // Forward/backward; need to normalize or small errors will quickly build up
        double theta = - cc.rotation.y * Mathd.Deg2Rad + Mathd.PI_PRECISE / 2;
        dir = new Vector3d(Mathd.Cos(theta), 0, Mathd.Sin(theta));
        pos = Hyper.hypNormalize(Hyper.lineDir(pos, dir, ms * verticalInput));

        // Right/left
        theta = theta - Mathd.PI_PRECISE / 2;
        right = new Vector3d(Mathd.Cos(theta), 0, Mathd.Sin(theta));
        pos = Hyper.hypNormalize(Hyper.lineDir(pos, right, ms * horizontalInput));
    }
}
