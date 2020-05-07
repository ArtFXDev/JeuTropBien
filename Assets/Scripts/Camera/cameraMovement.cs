using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovement : MonoBehaviour
{
    public float cameraSpeed = 0.01f;

    //Camera Boudaries
    [SerializeField]
    float boundariesXMin;

    [SerializeField]
    float boundariesXMax;

    [SerializeField]
    float boundariesYMin;

    [SerializeField]
    float boundariesYMax;

    [SerializeField]
    float boundariesZMin;

    [SerializeField]
    float boundariesZMax;


    void Update()
    {
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        //Check if we're touching the screen and moving the finger
        if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            //Get touched position
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

            //Move the camera with the finger movement
            transform.Translate(-touchDeltaPosition.x * cameraSpeed, -touchDeltaPosition.y * cameraSpeed, 0);

            //Check if camera position is Inside boundaries
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, boundariesXMin, boundariesXMax),
                Mathf.Clamp(transform.position.y, boundariesYMin, boundariesYMax),
                Mathf.Clamp(transform.position.z, boundariesZMax, boundariesZMin)
                );
        }
    }
}
