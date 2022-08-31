using UnityEngine;
using UnityEngine.InputSystem;

public class RightInputs : Inputs
{
    public static RightInputs Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
    }
}
