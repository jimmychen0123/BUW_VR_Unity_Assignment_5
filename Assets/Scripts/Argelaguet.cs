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
