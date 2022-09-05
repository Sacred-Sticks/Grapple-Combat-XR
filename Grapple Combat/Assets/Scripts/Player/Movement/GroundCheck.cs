using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private GroundCheckData groundCheckData;

    [System.Serializable] private struct GroundCheckData
    {
        public float groundDistance;
        public LayerMask groundLayer;
    }

    private BoxCollider groundingBox;

    private void Awake()
    {
        groundingBox = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        bool groundCheck = CheckGround();
        if (groundCheck && !groundingBox.enabled)
        {
            groundingBox.enabled = true;
        } else if (!groundCheck && groundingBox.enabled)
        {
            groundingBox.enabled = false;
        }
    }

    public bool CheckGround()
    {
        if (Physics.Raycast(transform.position + transform.up, -transform.up, groundCheckData.groundDistance, groundCheckData.groundLayer))
        {
            Debug.Log("Raycast true");
            return true;
        }
        Debug.Log("Raycast False");
        return false;
    }
}
