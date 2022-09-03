using System.Collections;
using UnityEngine;
using Autohand;
using System.Runtime.CompilerServices;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEditor.Build.Content;

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
    [SerializeField] private LayerMask grapplableLayers;
    [SerializeField] private GameObject player;
    [Header("Joint Data")]
    [SerializeField] private SpringData _springData;

    [System.Serializable] public struct SpringData
    {
        public float spring;
        public float damper;
        public float massScale;
        public float minDistance;
        public float maxDistance;
    }

    private SpringJoint joint;
    private LineRenderer lr;
    private Rigidbody playerBody;

    private Vector3 _grapplePoint;
    private bool _isGrappling = false;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        playerBody = player.GetComponent<Rigidbody>();
    }

    private IEnumerator Start()
    {
        // Separate to match the output with controller input, acts similar to Update Method
        if (leftController)
        {
            while (true)
            {
                CheckGrapple(LeftInputs.Instance);
                yield return new WaitForFixedUpdate();
            }
        }
        
        if (rightController)
        {
            while (true)
            {
                CheckGrapple(RightInputs.Instance);
                yield return new WaitForFixedUpdate();
            }
        }

        Debug.LogWarning("You need to select either left or right handed on " + gameObject.name);
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

        Debug.Log(grip + " grip value " + controllerInput.name);

        if (grip > inputThreshold && !_isGrappling)
        {
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
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 100, grapplableLayers))
        {
            _isGrappling = true;
            _grapplePoint = hit.point;
            joint = player.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = _grapplePoint;

            lr.positionCount = 2;

            AdjustJointSettings();
            //AdjustPlayerSettings();
        }
    }

    private void AdjustJointSettings()
    {
        float distance = Vector3.Distance(player.transform.position, _grapplePoint);

        joint.minDistance = distance * _springData.minDistance;
        joint.maxDistance = distance * _springData.maxDistance;

        joint.spring = _springData.spring;
        joint.damper = _springData.damper;
        joint.massScale = _springData.massScale;
    }

    private void AdjustPlayerSettings()
    {
        // Stop any Movement Constraints
        playerBody.constraints = RigidbodyConstraints.None;
        playerBody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void StopGrapple()
    {
        Destroy(joint);
        lr.positionCount = 0;
    }

    private void DrawRope()
    {
        lr.SetPosition(0, _grapplePoint);
        lr.SetPosition(1, transform.position);
    }
}
