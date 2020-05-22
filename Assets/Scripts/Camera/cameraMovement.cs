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

    [SerializeField]
    float boundariesOrthoMin;

    [SerializeField]
    float boundariesOrthoMax;

    [SerializeField]
    float boundariesOrthoDefault;


    void Update()
    {
        UpdateCamera();
    }

    void UpdateCamera()
    {
        //Zoom
        if (Input.touchCount == 2)
        {
            //Let's calcule the length between fingers
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            //Zoom
            Zoom(difference * 0.01f);
        }

        //Position
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
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

    //Zoom function
    void Zoom(float increment)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, boundariesOrthoMin, boundariesOrthoMax);
    }
}
