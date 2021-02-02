using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


public class DefaultRay : MonoBehaviour
{
    private GameObject scene = null;
    private GameObject rightHandController;
    private XRController rightXRController;
    private LineRenderer rightRayRenderer;
    private GameObject rightRayIntersectionSphere;
    private RaycastHit rightHit;
    public LayerMask myLayerMask;
    private GameObject selectedObject = null;

    private GameObject cameraOffset;
    private GameObject xrRig;
    private GameObject deselectedObject;
    private Matrix4x4 newMatrix;
    private Matrix4x4 initialMatrix;
    private bool initialButtonPress = true;


    private bool gripButtonLF = false;

    void Awake()
    {
        scene = GameObject.Find("Scene");
        rightHandController = GameObject.Find("RightHand Controller");

        if (rightHandController != null) // guard
        {
            rightXRController = rightHandController.GetComponent<XRController>();

            //rightRayRenderer = gameObject.AddComponent<LineRenderer>();

            rightRayRenderer = rightHandController.GetComponent<LineRenderer>();
            if (rightRayRenderer == null) rightRayRenderer = rightHandController.AddComponent<LineRenderer>() as LineRenderer;
            //rightRayRenderer.name = "Right Ray Renderer";
            rightRayRenderer.startWidth = 0.01f;
            rightRayRenderer.positionCount = 2; // two points (one line segment)
            rightRayRenderer.enabled = true;

            // geometry for intersection visualization
            rightRayIntersectionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //rightRayIntersectionSphere.transform.parent = this.gameObject.transform;
            rightRayIntersectionSphere.name = "Right Ray Intersection Sphere";
            rightRayIntersectionSphere.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            rightRayIntersectionSphere.GetComponent<MeshRenderer>().material.color = Color.yellow;
            rightRayIntersectionSphere.GetComponent<SphereCollider>().enabled = false; // disable for picking ?!
            rightRayIntersectionSphere.SetActive(false); // hide

            cameraOffset = GameObject.Find("Camera Offset");
            xrRig = GameObject.Find("XR Rig");
        }
    }


    // Update is called once per frame
    void Update()
    {

        // ----------------- ray intersection stuff -----------------
        // Does the ray intersect any objects
        if (Physics.Raycast(rightHandController.transform.position, rightHandController.transform.TransformDirection(Vector3.forward), out rightHit, Mathf.Infinity, myLayerMask))
        {
            //Debug.Log("Did Hit");
            // update ray visualization
            rightRayRenderer.SetPosition(0, rightHandController.transform.position);
            rightRayRenderer.SetPosition(1, rightHit.point);

            // update intersection sphere visualization
            rightRayIntersectionSphere.SetActive(true); // show
            rightRayIntersectionSphere.transform.position = rightHit.point;
        }
        else // ray does not intersect with objects
        {
            // update ray visualization
            rightRayRenderer.SetPosition(0, rightHandController.transform.position);
            rightRayRenderer.SetPosition(1, rightHandController.transform.position + rightHandController.transform.TransformDirection(Vector3.forward) * 1000);

            // update intersection sphere visualization
            rightRayIntersectionSphere.SetActive(false); // hide
        }

        //Dragging1(); // dragging version1: attach selected object to ray 
        Dragging2(); // dragging version2: move only virtually attached object dummy

    }

    private void Dragging1()
    {
        // mapping: grip button (middle finger)
        bool gripButton = false;
        rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out gripButton);
        //Debug.Log("middle finger rocker: " + gripButton);

        if (gripButton != gripButtonLF) // state changed
        {
            if (gripButton) // up (false->true)
            {
                if (rightHit.collider != null && selectedObject == null)
                {
                    SelectObject(rightHit.collider.gameObject);
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
        selectedObject = go;
        selectedObject.transform.SetParent(rightHandController.transform, false); // worldPositionStays = false

        // YOUR CODE - BEGIN
        // compensate position and orientation offset of the hit game object and the rightHandController to prevent jumps
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

        newMatrix = deviceMatInverse * xrRigMatInverse * sceneMat * localMat ;
        SetTransformByMatrix(selectedObject, newMatrix);

        // YOUR CODE - END
    }

    private void DeselectObject()
    {
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


         // O'1
        Matrix4x4 localMat = Matrix4x4.TRS(selectedObject.transform.localPosition, 
                                            selectedObject.transform.localRotation,
                                            selectedObject.transform.localScale);
        // Matrix4x4 localMatInverse = localMat.inverse;
    
        Matrix4x4 newMat = sceneMatInverse * xrRigMat * cameraMat * deviceMat * newMatrix;
        SetTransformByMatrix(selectedObject, newMat);
        // YOUR CODE - END

        selectedObject = null;
    }

    private void Dragging2()
    {
        // mapping: grip button (middle finger)
        bool gripButton = false;
        rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out gripButton);
        //Debug.Log("middle finger rocker: " + gripButton);

        // YOUR CODE - BEGIN
         if (gripButton != gripButtonLF) // state changed
        {
            if (gripButton) // up (false->true)
            {

                if (rightHit.collider != null && selectedObject == null)
                {

                    if (initialButtonPress) {
                        
                    selectedObject = rightHit.collider.gameObject;

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

                    initialMatrix = deviceMatInverse * cameraMatInverse * xrRigMatInverse * sceneMat * localMat ;
                
                    // initialMatrix = deviceMatInverse * xrRigMatInverse * sceneMat * localMat ;
                    SetTransformByMatrix(selectedObject, initialMatrix);

                    initialButtonPress = false;
                    } 
                } 
            }
            else // down (true->false)
            {
                initialButtonPress = true;
                selectedObject = null;
            }
        }

        if(!initialButtonPress && selectedObject != null) {
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


            // O'1
            // Matrix4x4 localMat = Matrix4x4.TRS(selectedObject.transform.localPosition, 
            //                                     selectedObject.transform.localRotation,
            //                                     selectedObject.transform.localScale);
            // Matrix4x4 localMatInverse = localMat.inverse;
        
            Matrix4x4 newMat = sceneMatInverse * xrRigMat * cameraMat * deviceMat * initialMatrix;
            SetTransformByMatrix(selectedObject, newMat);
        }

        // YOUR CODE - END

        gripButtonLF = gripButton;
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
        rightRayIntersectionSphere.SetActive(false);
    }

    private void OnEnable()
    {
        rightRayRenderer = rightHandController.GetComponent<LineRenderer>();
        if (rightRayRenderer == null) rightRayRenderer = rightHandController.AddComponent<LineRenderer>() as LineRenderer;
        rightRayRenderer.enabled = true;
    }

}
