using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

[RequireComponent(typeof(Rigidbody))]
public class WallRunning : MonoBehaviour
{
    public static WallRunning Instance;

    [SerializeField] private WallRunData wallRunData;
    [SerializeField] private WallJumpData wallJumpData;
    [SerializeField] private GravityData gravityData;

    [System.Serializable] private struct WallRunData
    {
        public float wallRunningSpeed;
        public float wallRunTimer;
        public float maxAngleDifference;
        public float sidePushVelocity;
    }
    [System.Serializable] private struct WallJumpData
    {
        public float wallJumpX;
        public float wallJumpY;
        public float wallJumpSpeed;
        public float minWallJumpX;
        public float wallJumpCooldown;
    }
    [System.Serializable] private struct GravityData
    {
        public float wallRunGravityMultiplier;
        public float standardGravityMultiplier;
    }

    private Rigidbody body;
    private Vector3 wallRunVelocity;
    private Transform cameraTransform;

    private float verticalVelocity, wallDirection, gravityMultiplier;
    private bool canWallRun = true, canWallJump = true, jumpedPreviously, jumped, isJumping;

    private Vector3 surfaceNormal, forwardDirection;

    private void Awake()
    {
        Instance = this;

        body = GetComponent<Rigidbody>();
        cameraTransform = FindObjectOfType<Camera>().gameObject.transform;
    }

    private void Start()
    {
        gravityMultiplier = gravityData.standardGravityMultiplier;
    }

    private void FixedUpdate()
    {
        GetJumpingInput();

        if (!CheckWallRunStatus()) 
        {
            return; 
        }

        // Set WallRun Timer
        if (canWallRun)
        {
            StartCoroutine(WallRunTimer());
        }

        SetMovementOverride(true);
        WallRun();
    }

    private void GetJumpingInput()
    {
        jumpedPreviously = jumped;
        float jumpInput = RightInputs.Instance.GetPrimary();
        jumped = jumpInput > 0.5f;
    }

    private void WallRun()
    {
        if (jumped && !jumpedPreviously)
        {
            // Jump off of the wall
            if (canWallJump)
            {
                StartCoroutine(JumpFromWall());
            }
        }
        else
        {
            // Run across the wall
            SetVerticalVelocity();
            GetDirections();
            SetCustomVelocity(
                (forwardDirection * wallRunData.wallRunningSpeed) +
                (Vector3.up * verticalVelocity) +
                (-surfaceNormal * wallRunData.sidePushVelocity));
            AutoHandPlayer.Instance.SetOverridingVelocity(wallRunVelocity);
        }
    }

    private bool CheckWallRunStatus()
    {
        if (isJumping) return false;

        // Cancel wallrun if Grounded or jointed
        if (AutoHandPlayer.Instance.CustomGroundCheck() || GetComponent<Joint>() != null)
        {
            SetMovementOverride(false);
            canWallRun = true;
            canWallJump = true;
            isJumping = false;
            StopCoroutine(JumpFromWall());
            return false;
        }

        surfaceNormal = CheckForWalls.Instance.GetSurfaceNormal();

        // Cancel wallrun if not near wall
        if (surfaceNormal == Vector3.zero)
        {
            SetMovementOverride(false);
            canWallRun = false;
            canWallJump = true;
            isJumping = false;
            StopCoroutine(JumpFromWall());
            return false;
        }


        return true;
    }

    private static void SetMovementOverride(bool isMovementOverridden)
    {
        if (AutoHandPlayer.Instance.GetIsMovementOverridden() != isMovementOverridden)
        {
            AutoHandPlayer.Instance.SetIsMovementOverridden(isMovementOverridden);
        }
    }

    private void SetVerticalVelocity()
    {
        if (body.velocity.y > 0)
        {
            verticalVelocity = body.velocity.y + Physics.gravity.y * Time.fixedDeltaTime * gravityData.standardGravityMultiplier;
            return;
        }
        verticalVelocity = body.velocity.y + Physics.gravity.y * Time.fixedDeltaTime * gravityMultiplier;
    }

    private void SetCustomVelocity(Vector3 velocity)
    {
        wallRunVelocity = velocity;
    }

    private void GetDirections()
    {
        surfaceNormal = new(surfaceNormal.x, 0, surfaceNormal.z);
        forwardDirection = new(surfaceNormal.z, 0, -surfaceNormal.x);
        Vector3 cameraForward = 
            (cameraTransform.forward.x * Vector3.right) + 
            (cameraTransform.forward.z * Vector3.forward);
        float angleDifference = Vector3.Angle(forwardDirection, cameraForward);
        if (angleDifference > wallRunData.maxAngleDifference)
        {
            forwardDirection *= -1;
        }
    }

    private IEnumerator WallRunTimer()
    {
        canWallRun = false;
        gravityMultiplier = gravityData.wallRunGravityMultiplier;
        yield return new WaitForSeconds(wallRunData.wallRunTimer);
        gravityMultiplier = gravityData.standardGravityMultiplier;
    }

    private IEnumerator JumpFromWall()
    {
        //Vector3 transformRight = (cameraTransform.right.x * Vector3.right + cameraTransform.right.z * Vector3.forward).normalized;
        //Vector3 transformForward = (cameraTransform.forward.x * Vector3.right + cameraTransform.forward.z * Vector3.forward);
        Vector3 jumpingVelocity = 
            (cameraTransform.forward * wallJumpData.wallJumpSpeed);

        //override jumpingVelocity based on surface normal if the player is looking towards the wall
        float sideMagnitude = Vector3.Dot(jumpingVelocity, surfaceNormal);
        if (sideMagnitude < wallJumpData.minWallJumpX)
        {
            jumpingVelocity =
                (forwardDirection * wallJumpData.wallJumpSpeed) +
                (surfaceNormal * wallJumpData.wallJumpX) +
                (Vector3.up * wallJumpData.wallJumpY);
        }

        AutoHandPlayer.Instance.SetOverridingVelocity(jumpingVelocity);
        isJumping = true;
        canWallJump = false;
        yield return new WaitForSeconds(wallJumpData.wallJumpCooldown);
        SetCustomVelocity(body.velocity);
        isJumping = false;
        canWallJump = true;
        SetMovementOverride(false);
    }
}
