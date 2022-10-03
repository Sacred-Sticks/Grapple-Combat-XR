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
        public float jumpHeight;
        public float jumpCooldown;
    }

    private float jumpForce;
    private bool canJump = true;

    private void Start()
    {
        float time = Mathf.Sqrt(2 * -jumpData.jumpHeight / Physics.gravity.y);
        float velocity = -Physics.gravity.y * time;
        jumpForce = velocity;


        if (inputData.leftController)
        {
            StartCoroutine(HandBasedUpdate(LeftInputs.Instance));
            return;
        }

        if (inputData.rightController)
        {
            StartCoroutine(HandBasedUpdate(RightInputs.Instance));
            return;
        }

        Debug.LogWarning("You need to select a hand for Jump.cs on " + gameObject.name);
    }

    private IEnumerator HandBasedUpdate(Inputs controllerInput)
    {
        while (true)
        {
            if (controllerInput.GetPrimary() > inputData.inputThreshold)
            {
                if (canJump)
                {
                    StartCoroutine(JumpTimer());
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator JumpTimer()
    {
        AutoHandPlayer.Instance.Jump(jumpForce);
        canJump = false;
        yield return new WaitForSeconds(jumpData.jumpCooldown);
        canJump = true;
    }
}
