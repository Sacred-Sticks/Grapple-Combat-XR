using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleAimGuide : MonoBehaviour
{
    [SerializeField] private InputData inputData;
    [Space]
    [SerializeField] private AimData aimData;
    [SerializeField] private GameObject player;

    [System.Serializable] private struct InputData
    {
        public bool leftController;
        public bool rightController;
        public float inputThreshold;
    }
    [System.Serializable] private struct AimData
    {
        public GameObject reticle;
        public float range;
        public LayerMask grappleLayer;
    }

    private GameObject aimReticle;
    private Vector3 reticlePos;

    private void Awake()
    {
        
    }

    private IEnumerator Start()
    {
        if (inputData.leftController)
        {
            while (true)
            {
                HandBasedUpdate(LeftInputs.Instance);
                yield return new WaitForEndOfFrame();
            }
        }
        if (inputData.rightController)
        {
            while (true)
            {
                HandBasedUpdate(RightInputs.Instance);
                yield return new WaitForEndOfFrame();
            }
        }

        Debug.LogWarning("You must select a hand for GrappleAimGuide.cs on " + gameObject.name);
        yield return new WaitForEndOfFrame();
    }

    private void HandBasedUpdate(Inputs controllerInput)
    {
        if (controllerInput.GetGrip() < inputData.inputThreshold)
        {
            if (GetRaycast())
            {
                if (aimReticle != null)
                {
                    aimReticle.transform.position = reticlePos;
                }
                else
                {
                    aimReticle = Instantiate(aimData.reticle, reticlePos, player.transform.rotation);
                }
            } else
            {
                Destroy(aimReticle);
            }

        } 
        else
        {
            if (aimReticle != null)
            {
                Destroy(aimReticle);
            }
        }
    }

    private bool GetRaycast()
    {
        RaycastHit hit = new();
        if (Physics.Raycast(transform.position, transform.forward, out hit, aimData.range, aimData.grappleLayer))
        {
            reticlePos = hit.point;
            return true;
        }
        return false;
    }
}
