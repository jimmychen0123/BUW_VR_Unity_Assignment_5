using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Argelaguet : MonoBehaviour
{
    private GameObject mainCamera;
    private GameObject rightHandController;

    private GameObject selectionRayGO;
    private LineRenderer selectionRayLR;

    private GameObject feedbackRayGO;
    private LineRenderer feedbackRayLR;

    private GameObject rightRayIntersectionSphere;

    private RaycastHit rightHit;
    public LayerMask myLayerMask;


    private void Awake()
    {
        mainCamera = GameObject.Find("Main Camera");
        rightHandController = GameObject.Find("RightHand Controller");

        if (selectionRayGO == null)
        {
            selectionRayGO = new GameObject();
            selectionRayGO.name = "Selection Ray";
            selectionRayGO.transform.SetParent(rightHandController.transform, false);

            selectionRayLR = selectionRayGO.AddComponent<LineRenderer>() as LineRenderer;
            selectionRayLR.startWidth = 0.01f;
            selectionRayLR.positionCount = 2; // two points (one line segment)
            selectionRayLR.enabled = false;
            selectionRayLR.material.color = Color.white;
        }

        if (feedbackRayGO == null)
        {
            feedbackRayGO = new GameObject();
            feedbackRayGO.name = "Feedback Ray";
            feedbackRayGO.transform.SetParent(rightHandController.transform, false);

            feedbackRayLR = feedbackRayGO.AddComponent<LineRenderer>() as LineRenderer;
            feedbackRayLR.startWidth = 0.01f;
            feedbackRayLR.positionCount = 2; // two points (one line segment)
            feedbackRayLR.enabled = false;
        }

        if (rightRayIntersectionSphere == null)
        {
            // geometry for intersection visualization
            rightRayIntersectionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //rightRayIntersectionSphere.transform.parent = this.gameObject.transform;
            rightRayIntersectionSphere.name = "Right Ray Intersection Sphere";
            rightRayIntersectionSphere.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            rightRayIntersectionSphere.GetComponent<MeshRenderer>().material.color = Color.yellow;
            rightRayIntersectionSphere.GetComponent<SphereCollider>().enabled = false; // disable for picking ?!
            rightRayIntersectionSphere.SetActive(false); // hide
        }
        
    }


    // Update is called once per frame
    void Update()
    {
        // ----------------- Argelaguet selection technique -----------------

        // YOUR CODE - BEGIN
        // compute selection and visualize selection ray

        //selection ray starts at eye(main camera), but direction of ray is defined by hand controller
        if (Physics.Raycast(mainCamera.transform.position, rightHandController.transform.TransformDirection(Vector3.forward), out rightHit, Mathf.Infinity))
        {
            Debug.Log("Did Hit");
            // update ray visualization
            selectionRayLR.SetPosition(0, mainCamera.transform.position);
            selectionRayLR.SetPosition(1, rightHit.point);

            feedbackRayLR.SetPosition(0, rightHandController.transform.position);
            feedbackRayLR.SetPosition(1, rightHit.point);

            // update intersection sphere visualization
            rightRayIntersectionSphere.SetActive(true); // show
            rightRayIntersectionSphere.transform.position = rightHit.point;
        }
        else // ray does not intersect with objects
        {
            // update ray visualization
            selectionRayLR.SetPosition(0, mainCamera.transform.position);
            selectionRayLR.SetPosition(1, rightHandController.transform.position + rightHandController.transform.TransformDirection(Vector3.forward) * 1000);

            feedbackRayLR.SetPosition(0, rightHandController.transform.position);
            feedbackRayLR.SetPosition(1, rightHandController.transform.position + rightHandController.transform.TransformDirection(Vector3.forward) * 1000);

            // update intersection sphere visualization
            rightRayIntersectionSphere.SetActive(false); // hide
        }

        // visualize feedback ray
        // YOUR CODE - END
    }

    private void OnDisable()
    {
        if (selectionRayLR != null) selectionRayLR.enabled = false;
        if (feedbackRayLR != null) feedbackRayLR.enabled = false;
        if (rightRayIntersectionSphere != null) rightRayIntersectionSphere.SetActive(false);
    }

    private void OnEnable()
    {
        if (selectionRayLR != null) selectionRayLR.enabled = true;
        if (feedbackRayLR != null) feedbackRayLR.enabled = true;
    }

}

/*break down usability into the following goals:
 * 1. effectiveness: this technique is not affected by the eye-hand visibility mismatch
   2. efficiency: this technique outperform raycasting in complex scenes where visibility plays an important role
   3. learnability: this technique may not be intuitive enough to new users since the selection ray starts from head but controlled by hand.
 */
