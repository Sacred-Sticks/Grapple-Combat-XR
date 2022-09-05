using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    //public static GroundCheck Instance;

    [SerializeField] private GroundCheckData groundCheckData;

    private struct GroundCheckData
    {
        public float groundDistance;
        public LayerMask groundLayer;
    }

    private void Awake()
    {
        //Instance = this;
    }

    public bool CheckGround()
    {
        if (Physics.Raycast(transform.position, -transform.up, groundCheckData.groundDistance, groundCheckData.groundLayer))
        {
            return true;
        }
        return false;
    }
}
