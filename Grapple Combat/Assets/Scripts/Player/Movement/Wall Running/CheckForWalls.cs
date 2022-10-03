using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForWalls : MonoBehaviour
{
    public static CheckForWalls Instance;

    [SerializeField] private WallCheckData wallCheckData;

    [System.Serializable] private struct WallCheckData
    {
        public float radius;
        public float maxDistance;
        public LayerMask wallLayermask;
    }

    private Transform cameraTransform;

    private int wallDirection;
    private Vector3 surfaceNormal;

    private void Awake()
    {
        Instance = this;

        cameraTransform = FindObjectOfType<Camera>().gameObject.transform;
    }

    public void FindWallDirection()
    {
        wallDirection = 0;
        surfaceNormal = Vector3.zero;
        Vector3 cameraRight = (cameraTransform.right.x * Vector3.right + cameraTransform.right.z * Vector3.forward).normalized;
        RaycastHit hit;

        if (Physics.SphereCast(transform.position + transform.up, wallCheckData.radius, (cameraRight),
            out hit, wallCheckData.maxDistance, wallCheckData.wallLayermask))
        {
            wallDirection += 1;
            surfaceNormal = hit.normal;
        }
        if (Physics.SphereCast(transform.position + transform.up, wallCheckData.radius, (-cameraRight),
            out hit, wallCheckData.maxDistance, wallCheckData.wallLayermask))
        {
            wallDirection += -1;
            surfaceNormal = hit.normal;
        }
    }

    public Vector3 GetSurfaceNormal()
    {
        FindWallDirection();
        return surfaceNormal;
    }
}
