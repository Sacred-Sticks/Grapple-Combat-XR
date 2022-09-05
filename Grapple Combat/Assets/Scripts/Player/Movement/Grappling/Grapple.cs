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
    [SerializeField] private InputData inputData;
    [Space]
    [SerializeField] private GrappleData grappleData;
    [Space]
    [SerializeField] private SpringData springData;

    [System.Serializable] private struct InputData
    {
        public bool leftController;
        public bool rightController;
        public float inputThreshold;
    }
    [System.Serializable] private struct GrappleData
    {
        public LayerMask grapplableLayers;
        public GameObject player;
        public float maxRange;
    }
    [System.Serializable] private struct SpringData
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
        playerBody = grappleData.player.GetComponent<Rigidbody>();
    }

    private IEnumerator Start()
    {
        // Separate to match the output with controller input, acts similar to Update Method
        if (inputData.leftController)
        {
            while (true)
            {
                HandBasedUpdate(LeftInputs.Instance);
                yield return new WaitForFixedUpdate();
            }
        }
        
        if (inputData.rightController)
        {
            while (true)
            {
                HandBasedUpdate(RightInputs.Instance);
                yield return new WaitForFixedUpdate();
            }
        }

        Debug.LogWarning("You must select a hand for Grapple.cs on " + gameObject.name);
    }

    private void HandBasedUpdate(Inputs controllerInput)
    {
        CheckGrapple(controllerInput);
        if (joint != null)
            joint.anchor = transform.position - grappleData.player.transform.position;
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

        if (grip > inputData.inputThreshold && !_isGrappling)
        {
            StartGrapple();
        }
        else if (grip < inputData.inputThreshold && _isGrappling)
        {
            _isGrappling = false;
            StopGrapple();
        }
    }

    private void StartGrapple()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, grappleData.maxRange, grappleData.grapplableLayers))
        {
            _isGrappling = true;
            _grapplePoint = hit.point;
            joint = grappleData.player.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = _grapplePoint;

            lr.positionCount = 2;

            AdjustJointSettings();
        }
    }

    private void AdjustJointSettings()
    {
        float distance = Vector3.Distance(grappleData.player.transform.position, _grapplePoint);

        joint.minDistance = distance * springData.minDistance;
        joint.maxDistance = distance * springData.maxDistance;

        joint.spring = springData.spring;
        joint.damper = springData.damper;
        joint.massScale = springData.massScale;
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
