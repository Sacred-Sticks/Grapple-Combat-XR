using System.Collections;
using UnityEditorInternal;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Grapple : MonoBehaviour
{
    [Header("Input Data")]
    [SerializeField] private bool leftController;
    [SerializeField] private bool rightController;
    [Space]
    [SerializeField] private float inputThreshold = 0.5f;
    [Space]
    [Header("Grapple Data")]
    [SerializeField] private Transform grappleSource;
    [SerializeField] private LayerMask grapplableLayers;
    [SerializeField] private GameObject player;
    [SerializeField] private float _maxDistanceMultiplier, _minDistanceMultiplier, _spring, _damper, _massScale;

    private SpringJoint joint;
    private LineRenderer lr;

    private Vector3 _grapplePoint;
    private bool _isGrappling = false;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    private IEnumerator Start()
    {
        // Separate to match the output with controller input, acts similar to Update Method

        if (leftController)
        {
            while (true)
            {
                //CheckGrapple(LeftInputs.Instance);
                yield return new WaitForEndOfFrame();
            }
        }
        
        if (rightController)
        {
            Debug.Log("Right Controller Active");
            while (true)
            {
                CheckGrapple(RightInputs.Instance);
                yield return new WaitForEndOfFrame();
            }
        }

        Debug.Log("You need to select either left or right handed on " + gameObject.name);
    }

    private void LateUpdate()
    {
        if (joint != null)
        {
            DrawRope();
        }
    }

    private void CheckGrapple(Inputs controllerInput)
    {
        float grip = controllerInput.GetGrip();
        //Debug.Log(grip + " grip value");

        if (grip > inputThreshold && !_isGrappling)
        {
            _isGrappling = true;
            StartGrapple();
        }
        else if (grip < inputThreshold && _isGrappling)
        {
            _isGrappling = false;
            StopGrapple();
        }
    }

    private void StartGrapple()
    {
        RaycastHit hit;
        Debug.Log("Starting Grapple");
        if (Physics.Raycast(grappleSource.position, grappleSource.forward, out hit, 100, grapplableLayers))
        {
            Debug.Log("Target Hit!");
            _grapplePoint = hit.point;
            joint = player.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = _grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.transform.position, _grapplePoint);

            joint.maxDistance = distanceFromPoint * _maxDistanceMultiplier;
            joint.minDistance = distanceFromPoint * _minDistanceMultiplier;

            joint.spring = _spring;
            joint.damper = _damper;
            joint.massScale = _massScale;

            lr.positionCount = 2;
        } else
        {
            Debug.Log("Missed Shot");
        }
    }

    public void StopGrapple()
    {
        Destroy(joint);
        lr.positionCount = 0;
    }

    private void DrawRope()
    {
        lr.SetPosition(0, _grapplePoint);
        lr.SetPosition(1, grappleSource.position);
    }
}
