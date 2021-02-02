using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DepthRay : MonoBehaviour
{
    private GameObject scene = null;
    private GameObject rightHandController;
    private XRController rightXRController;
    private GameObject cursorBall;
    //private Collider cursorCollider;
    private CollisionDetector collisionDetector;

    private LineRenderer rightRayRenderer;
    private GameObject selectedObject;

    private bool rayOnFlag = false;
    public float cursorSpeed = 0.5f;
    private float cursorDistance = 0.2f;

    private bool gripButtonLF = false;

    //YOUR CODE - BEGIN

    float speed = 1.0f;
    //YOUR CODE - END

    void Awake()
    {
        scene = GameObject.Find("Scene");
        rightHandController = GameObject.Find("RightHand Controller");

        if (rightHandController != null) // guard
        {
            rightXRController = rightHandController.GetComponent<XRController>();
            rightRayRenderer = rightHandController.GetComponent<LineRenderer>();
            if (rightRayRenderer == null) rightRayRenderer = rightHandController.AddComponent<LineRenderer>() as LineRenderer;
            
            rightRayRenderer.startWidth = 0.01f;
            rightRayRenderer.positionCount = 2; // two points (one line segment)
            rightRayRenderer.enabled = true;

            cursorDistance = 0.2f;

            // geometry for intersection visualization
            cursorBall = GameObject.Find("CursorSphere");
            collisionDetector = cursorBall.GetComponent<CollisionDetector>();
            cursorBall.transform.position = rightHandController.transform.position + rightHandController.transform.forward * cursorDistance;
            cursorBall.SetActive(false);
        }

        //Debug.Log("DefaultRay Start: " + rightHandController);
    }

    // Update is called once per frame
    void Update()
    {

        if (rightHandController != null) // guard
        {
            // mapping: joystick
            Vector2 joystick;
            rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystick);
            //Debug.Log("Joystick: x" + joystick.x + " y" + joystick.y);

            // Move Cursor Selection Marker
            // YOUR CODE - BEGIN
            if(joystick.y > 0.005f && cursorBall.transform.localPosition.z <= 10.0f)
            {
                cursorBall.transform.Translate(0.0f, 0.0f, speed * joystick.y * Time.deltaTime, Space.Self);


            }

            if(joystick.y < -0.005f && cursorBall.transform.localPosition.z >=0.2)
            {

                cursorBall.transform.Translate(0.0f, 0.0f, speed * joystick.y * Time.deltaTime, Space.Self);

            }
            // YOUR CODE - END   

            UpdateRayVisualization(true);

        }

        // mapping: grip button (middle finger)
        bool gripButton = false;
        rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out gripButton);
        //Debug.Log("middle finger rocker: " + gripButton);

        if (gripButton != gripButtonLF) // state changed
        {
            if (gripButton) // up (false->true)
            {
                
                if (collisionDetector.collided && selectedObject == null)
                {
                    Debug.Log("Collided with Object: " + collisionDetector.collidedObject.name);
                    // YOUR CODE - BEGIN
                    SelectObject(collisionDetector.collidedObject);
                    
                    // YOUR CODE - END   
                }

            }
            else // down (true->false)
            {
                if (selectedObject != null)
                {
                    DeselectObject();
                }
            }
        }
        gripButtonLF = gripButton;
    }


    private void SelectObject(GameObject go)
    {
        // YOUR CODE - BEGIN
        selectedObject = go;
        selectedObject.transform.SetParent(cursorBall.transform, true);
        // YOUR CODE - END  
    }

    private void DeselectObject()
    {
        // YOUR CODE - BEGIN
        
        selectedObject.transform.SetParent(scene.transform, true);
        selectedObject = null;
        // YOUR CODE - END  
    }

    private void UpdateRayVisualization(bool rayFlag)
    {
        // Turn on ray visualization
        if (rayFlag && rayOnFlag == false)
        {
            rightRayRenderer.enabled = true;
            cursorBall.SetActive(true);
            cursorBall.GetComponent<SphereCollider>().enabled = true;
            rayOnFlag = true;
        }
        
        // update ray length and intersection point of ray
        if (rayOnFlag)
        { // if ray is on
            rightRayRenderer.SetPosition(0, rightHandController.transform.position);
            rightRayRenderer.SetPosition(1, rightHandController.transform.position + rightHandController.transform.forward * 10.0f);

            // Update the CursorSphere position every frame
            // YOUR CODE - BEGIN
            
            // YOUR CODE - END  
        }
    }


    private void OnDisable()
    {
        rightRayRenderer.enabled = false;
        cursorBall.SetActive(false);
        cursorBall.GetComponent<SphereCollider>().enabled = false;
        rayOnFlag = false;
    }
}
