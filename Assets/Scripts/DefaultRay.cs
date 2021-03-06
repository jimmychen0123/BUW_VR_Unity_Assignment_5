﻿using System.Collections;
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

    private bool gripButtonLF = false;

    // YOUR CODE - BEGIN
    //For dragging1(): store the original world transform of selected game object
    private Matrix4x4 selectionInitialWT;

    //For dragging2(): this is used to assign a child gameobject of input device(rightHand controller) as virtually attaching selected game object to input device
    private GameObject virtualObject;
    // YOUR CODE - END

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

        }
        //YOUR CODE - BEGIN
        //When application starts, assign a child gameobject of input device as selected gameobject virtually attached to input device 
        virtualObject = GameObject.Find("[RightHand Controller] Model");
        //YOUR CODE - END
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
        //YOUR CODE - BEGIN

        //store the selection's original world transform before setting it in the same position but relative to the NEW parent node
        selectionInitialWT = go.transform.localToWorldMatrix;

        //YOUR CODE - END

        selectedObject = go;
        selectedObject.transform.SetParent(rightHandController.transform, false); // worldPositionStays = true

        // YOUR CODE - BEGIN
        // compensate position and orientation offset of the hit game object and the rightHandController to prevent jumps
        
        //set the selection in the position of its original world transform and then selection moves with input device
        selectedObject.transform.position = selectionInitialWT.GetColumn(3);
        selectedObject.transform.rotation = selectionInitialWT.rotation;
        selectedObject.transform.localScale = selectionInitialWT.lossyScale;

        // YOUR CODE - END
    }

    private void DeselectObject()
    {
        // YOUR CODE - BEGIN
        // compensate for jumps of the selected object when reinserting to the scene-branch

        //store the selection's world transform after being moved around by input device
        selectionInitialWT = selectedObject.transform.localToWorldMatrix;
        // YOUR CODE - END

        selectedObject.transform.SetParent(scene.transform, false); // worldPositionStays = true


        // YOUR CODE - BEGIN
        // compensate for jumps of the selected object when reinserting to the scene-branch
        
        //set the selection in the position of where it was moved by input device
        selectedObject.transform.position = selectionInitialWT.GetColumn(3);
        selectedObject.transform.rotation = selectionInitialWT.rotation;
        selectedObject.transform.localScale = selectionInitialWT.lossyScale;

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
                    //store the selected game object
                    selectedObject = rightHit.collider.gameObject;
                    //store the selection's world transform before being moved around by input device
                    selectionInitialWT = rightHit.collider.gameObject.transform.localToWorldMatrix;
                    
                    //set the child gameobject of input device in the same tranform as selection
                    virtualObject.transform.position = selectionInitialWT.GetColumn(3);
                    virtualObject.transform.rotation = selectionInitialWT.rotation;
                    virtualObject.transform.localScale = selectionInitialWT.lossyScale;

                    
                }
                
            }
            else // down (true->false)
            {
                if (selectedObject != null)
                {
                    //DeselectObject();
                    selectedObject = null;
                }
            }
        }
        
        else if(gripButton) //when user still press the button and have selection moved by input device
        {
            //now the selection is moved as the virtual game object
            selectedObject.transform.position = virtualObject.transform.localToWorldMatrix.GetColumn(3);
            selectedObject.transform.rotation = virtualObject.transform.localToWorldMatrix.rotation;
            selectedObject.transform.localScale = virtualObject.transform.localToWorldMatrix.lossyScale;


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
