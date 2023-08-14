using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController.Ball;
public class MouseDrag : MonoBehaviour
{
    public float Strength = 1;

    private Camera cam;
    private BallController ballController;
    private Vector3 _mousePos;
    // Start is called before the first frame update
    void Start()
    {
        ballController = transform.GetComponent<BallController>();
        cam = ballController.cam;
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        this.gameObject.GetComponent<LineRenderer>().SetPosition(0, transform.position);

        if (Input.GetMouseButton(0))
        {
            
            Vector2 mPos = Input.mousePosition;
            _mousePos = (cam.ScreenToWorldPoint(new Vector3(mPos.x, mPos.y, Vector3.Distance(cam.transform.position, transform.position))));
            this.gameObject.GetComponent<LineRenderer>().SetPosition(1, _mousePos);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ballController.AddVelocity((_mousePos - transform.position) * Strength);
        }
        else
        {
            this.gameObject.GetComponent<LineRenderer>().SetPosition(1, transform.position);
            _mousePos = Vector3.zero;
        }
    }

}
