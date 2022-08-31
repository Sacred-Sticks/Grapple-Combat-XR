using UnityEngine;
using UnityEngine.InputSystem;

public class RightInputs : Inputs
{
    public static RightInputs Instance;

    private void Awake()
    {
        Instance = this;
        SetInputs();
    }
}
