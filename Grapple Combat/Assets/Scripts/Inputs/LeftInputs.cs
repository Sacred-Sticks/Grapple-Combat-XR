using UnityEngine;
using UnityEngine.InputSystem;

public class LeftInputs : Inputs
{
    public static LeftInputs Instance;

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
