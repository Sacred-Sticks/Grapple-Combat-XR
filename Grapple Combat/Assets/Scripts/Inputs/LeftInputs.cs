using UnityEngine;
using UnityEngine.InputSystem;

public class LeftInputs : Inputs
{
    public static LeftInputs Instance;

    private void Awake()
    {
        Instance = this;
    }
}
