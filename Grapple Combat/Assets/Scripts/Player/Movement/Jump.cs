using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    [SerializeField] private InputData inputData;
    [SerializeField] private JumpData jumpData;

    [System.Serializable] private struct InputData
    {
        public bool leftController;
        public bool rightController;
        public float inputThreshold;
    }
    [System.Serializable] private struct JumpData
    {
        public float jumpForce;
        public float jumpCooldown;
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

        Debug.Log("You need to select a hand for Jump.cs on " + gameObject.name);
    }

    private void HandBasedUpdate(Inputs controllerInput)
    {
        Debug.Log("Primary Value of Jumping is " + controllerInput.GetPrimary());
        if (controllerInput.GetPrimary() > inputData.inputThreshold)
        {
            AutoHandPlayer.Instance.Jump(jumpData.jumpForce);
            Debug.Log("Jumped");
        }
    }
}
