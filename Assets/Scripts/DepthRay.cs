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

    private Vector3[] positions;
    private Vector3[] pos;
    private int index = 0;
    public float speed;
    public float moveSpeed;
    private GameObject cameraOffset;
    private GameObject xrRig;
    private GameObject deselectedObject;
    private Matrix4x4 newMatrix;
    private Matrix4x4 initialMatrix;
    private bool initialButtonPress = true;


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

            cameraOffset = GameObject.Find("Camera Offset");
            xrRig = GameObject.Find("XR Rig");
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
            // Debug.Log("Joystick: x" + joystick.x + " y" + joystick.y);
            // joystick = new Vector2(joystick.x, joystick.y);


            // Move Cursor Selection Marker
            // YOUR CODE - BEGIN
            speed = 1.0f;
            moveSpeed = 1.0f;
            

            positions = new Vector3[rightRayRenderer.positionCount];
            // rightRayRenderer.GetPositions(positions);
            // Debug.Log("POSITIONS" + positions);

            // pos = positions;


            if (joystick.y > 0) { 
                cursorBall.transform.position = Vector3.MoveTowards(cursorBall.transform.position,
                                                                rightHandController.transform.position + rightHandController.transform.forward * 10.0f,
                                                                speed  * Time.deltaTime);
            } 

             if (joystick.y < 0) { 
                cursorBall.transform.position = Vector3.MoveTowards(cursorBall.transform.position,
                                                                rightHandController.transform.position,
                                                                speed * Time.deltaTime);
            }
            // cursorBall.transform.position = Vector3.Lerp(pos[index],
            //                                                     pos[index+1],
            //                                                     moveSpeed-index);

            // Debug.Log("Position index: " + pos[index]);
            // Debug.Log("Ball position: " + cursorBall.transform.position);
            // if (cursorBall.transform.position == pos[index]) {
            // if (joystick.y > 0) { 
            //     index += 1;
            // }

    
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
        selectedObject.transform.SetParent(cursorBall.transform, false); // worldPositionStays = false
        
        // Inverse of Cursor Sphere
        Matrix4x4 cursorMat = Matrix4x4.TRS(cursorBall.transform.localPosition, 
                                           cursorBall.transform.localRotation,
                                           cursorBall.transform.localScale);
        Matrix4x4 cursorMatInverse = cursorMat.inverse;
        // Inverse of RightHandController
        Matrix4x4 deviceMat = Matrix4x4.TRS(rightHandController.transform.localPosition, 
                                           rightHandController.transform.localRotation,
                                           rightHandController.transform.localScale);
        Matrix4x4 deviceMatInverse = deviceMat.inverse;

        // Inverse of Camera Offset
        Matrix4x4 cameraMat = Matrix4x4.TRS(cameraOffset.transform.localPosition, 
                                           cameraOffset.transform.localRotation,
                                           cameraOffset.transform.localScale);
        Matrix4x4 cameraMatInverse = cameraMat.inverse;

        // Inverse of XR Rig
        Matrix4x4 xrRigMat = Matrix4x4.TRS(xrRig.transform.localPosition, 
                                           xrRig.transform.localRotation,
                                           xrRig.transform.localScale);
        Matrix4x4 xrRigMatInverse = xrRigMat.inverse;

        // Scene
        Matrix4x4 sceneMat = Matrix4x4.TRS(scene.transform.localPosition, 
                                           scene.transform.localRotation,
                                           scene.transform.localScale);

        // O
        Matrix4x4 localMat = Matrix4x4.TRS(selectedObject.transform.localPosition, 
                                           selectedObject.transform.localRotation,
                                           selectedObject.transform.localScale);

        newMatrix = cursorMatInverse * deviceMatInverse * xrRigMatInverse * sceneMat * localMat ;
        SetTransformByMatrix(selectedObject, newMatrix);


        // YOUR CODE - END  
    }

    private void DeselectObject()
    {
        // YOUR CODE - BEGIN
        selectedObject.transform.SetParent(scene.transform, false); // worldPositionStays = false
        // YOUR CODE - BEGIN
        // compensate for jumps of the selected object when reinserting to the scene-branch
        // Inverse of Scene
        Matrix4x4 sceneMat = Matrix4x4.TRS(scene.transform.localPosition, 
                                           scene.transform.localRotation,
                                           scene.transform.localScale);
        Matrix4x4 sceneMatInverse = sceneMat.inverse;

        // XR Rig
        Matrix4x4 xrRigMat = Matrix4x4.TRS(xrRig.transform.localPosition, 
                                           xrRig.transform.localRotation,
                                           xrRig.transform.localScale);

        // Camera Offset
        Matrix4x4 cameraMat = Matrix4x4.TRS(cameraOffset.transform.localPosition, 
                                           cameraOffset.transform.localRotation,
                                           cameraOffset.transform.localScale);


        // RightHandController
        Matrix4x4 deviceMat = Matrix4x4.TRS(rightHandController.transform.localPosition, 
                                           rightHandController.transform.localRotation,
                                           rightHandController.transform.localScale);
        // Cursor Sphere
        Matrix4x4 cursorMat = Matrix4x4.TRS(cursorBall.transform.localPosition, 
                                           cursorBall.transform.localRotation,
                                           cursorBall.transform.localScale);

         // O'1
        Matrix4x4 localMat = Matrix4x4.TRS(selectedObject.transform.localPosition, 
                                            selectedObject.transform.localRotation,
                                            selectedObject.transform.localScale);
        // Matrix4x4 localMatInverse = localMat.inverse;
    
        Matrix4x4 newMat = sceneMatInverse * xrRigMat * cameraMat * deviceMat * cursorMat * newMatrix;
        SetTransformByMatrix(selectedObject, newMat);
        // YOUR CODE - END

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

    void SetTransformByMatrix(GameObject go, Matrix4x4 mat) // helper function
    {
        go.transform.localPosition = mat.GetColumn(3);
        go.transform.localRotation = mat.rotation;
        go.transform.localScale = mat.lossyScale;
    }


    private void OnDisable()
    {
        rightRayRenderer.enabled = false;
        cursorBall.SetActive(false);
        cursorBall.GetComponent<SphereCollider>().enabled = false;
        rayOnFlag = false;
    }
}
