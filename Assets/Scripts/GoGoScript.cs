using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class GoGoScript : MonoBehaviour
{
    private GameObject scene = null;
    private GameObject rightHandController;
    private XRController rightXRController;

    public float threshhold = 0.35f;
    private GameObject head;
    private GameObject leftHand;
    private GameObject rightHand;
    private CollisionDetector leftDetector;
    private CollisionDetector rightDetector;
    private GameObject leftHandCenter;
    private GameObject rightHandCenter;
    private GameObject rightHandColliderProxy;

    private GameObject selectedObject;

    private bool gripButtonLF = false;
    //YOUR CODE BEGIN
    private Vector3 headToRightHandCenter;
    public float distanceOnXZPlane;
    public float IsomorphicDistance;
    public float nonIsomorphicDistance;
    private float k = 25.0f;

    private float speed = 1.0f;

    //YOUR CODE END
    // Start is called before the first frame update
    void Awake()
    {
        scene = GameObject.Find("Scene");
        rightHandController = GameObject.Find("RightHand Controller");
        rightXRController = rightHandController.GetComponent<XRController>();

        head = transform.Find("Camera Offset/Main Camera").gameObject;
        leftHand = transform.Find("Camera Offset/LeftHand Controller/HandLeft").gameObject;
        rightHand = transform.Find("Camera Offset/RightHand Controller/HandRight").gameObject;
        leftHandCenter = transform.Find("Camera Offset/LeftHand Controller/LeftCenter").gameObject;
        rightHandCenter = transform.Find("Camera Offset/RightHand Controller/RightCenter").gameObject;

        rightHandColliderProxy = GameObject.Find("HandColliderProxy");
        rightDetector = rightHandColliderProxy.GetComponent<CollisionDetector>();
    }

    // Update is called once per frame
    void Update()
    {
        // YOUR CODE - BEGIN
        // GoGo behavior

        //compute the distance between RightCenter and MainCamera in the XZ plane
        //https://answers.unity.com/questions/51721/distance-between-2-objects-without-y.html
        headToRightHandCenter = rightHandCenter.transform.position - head.transform.position;
        headToRightHandCenter.y = 0;
        distanceOnXZPlane = headToRightHandCenter.magnitude;

        //get the distance between rightCenter and MainCamera 
        IsomorphicDistance = Vector3.Distance(rightHandCenter.transform.position, head.transform.position);

        if(distanceOnXZPlane > threshhold) //apply non-isomorphic distance scaling
        {

            //compute the non isomorphic distance
            nonIsomorphicDistance = IsomorphicDistance + k * Mathf.Pow(IsomorphicDistance - threshhold, 2);

            //compute the rightHand's position with keeping distance from head by using MoveTowards to have smooth transition
            //https://docs.unity3d.com/ScriptReference/Vector3.MoveTowards.html
            //https://answers.unity.com/questions/292084/keeping-distance-between-two-gameobjects.html
            rightHand.transform.position = Vector3.MoveTowards(rightHand.transform.position, (rightHandCenter.transform.position - head.transform.position).normalized * nonIsomorphicDistance + rightHandCenter.transform.position, 1.0f * Time.deltaTime);

            //rightHandCollider follow along the rightHand
            rightHandColliderProxy.transform.position = Vector3.MoveTowards(rightHand.transform.position, (rightHandCenter.transform.position - head.transform.position).normalized * nonIsomorphicDistance + rightHandCenter.transform.position, 1.0f * Time.deltaTime); ;

        }
        else //apply isomorphic distance scaling, assuming rightHandCenter(controller movement) to physical hand is 1:1
        {
            rightHand.transform.position = Vector3.MoveTowards(rightHand.transform.position, rightHandCenter.transform.position, 1.0f * Time.deltaTime);
            rightHandColliderProxy.transform.position = Vector3.MoveTowards(rightHandColliderProxy.transform.position, rightHandCenter.transform.position, 1.0f * Time.deltaTime);

        }
        // YOUR CODE - END   

        // mapping: grip button (middle finger)
        bool gripButton = false;
        rightXRController.inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out gripButton);
        //Debug.Log("middle finger rocker: " + gripButton);

        if (gripButton != gripButtonLF) // state changed
        {
            if (gripButton) // up (false->true)
            {
                if (rightDetector.collided && selectedObject == null)
                {
                    Debug.Log("Collided with Object: " + rightDetector.collidedObject.name);
                    // YOUR CODE - BEGIN
                    SelectObject(rightDetector.collidedObject);
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
        
        selectedObject.transform.SetParent(rightHand.transform, true);

        // Update the hand collider'color if selecting object sucessfully
        rightHandColliderProxy.GetComponent<MeshRenderer>().material.color = Color.yellow;
        
        
        // YOUR CODE - END  
    }

    private void DeselectObject()
    {
        // YOUR CODE - BEGIN
        selectedObject.transform.SetParent(scene.transform, true);
        selectedObject = null;
        rightHandColliderProxy.GetComponent<MeshRenderer>().material.color = Color.magenta;
        // YOUR CODE - END  
    }

    private void OnDisable()
    {
        rightHand.transform.position = rightHandCenter.transform.position;
        leftHand.transform.position = leftHandCenter.transform.position;
        rightHandColliderProxy.GetComponent<BoxCollider>().enabled = false;
        rightHandColliderProxy.SetActive(false);
    }

    private void OnEnable()
    {
        //rightHandCenter.transform.position = rightHandCenter.transform.parent.position;
        rightHand.transform.position = rightHandCenter.transform.position;
        leftHand.transform.position = leftHandCenter.transform.position;
        rightHandColliderProxy.GetComponent<BoxCollider>().enabled = true;
        rightHandColliderProxy.SetActive(true);
    }
}
