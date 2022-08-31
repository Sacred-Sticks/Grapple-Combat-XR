using UnityEngine;
using UnityEngine.InputSystem;

public class Inputs : MonoBehaviour
{
    [SerializeField] protected InputActionAsset playerInputAsset;
    [SerializeField] protected string actionMap;
    [Space]
    [SerializeField] protected string gripName;
    [SerializeField] protected string triggerName;
    [SerializeField] protected string primaryName;
    [SerializeField] protected string secondaryName;

    protected InputAction gripAction, triggerAction, primaryAction, secondaryAction;
    protected float gripValue, triggerValue, primaryValue, secondaryValue;

    private void Awake()
    {
        var map = playerInputAsset.FindActionMap(actionMap);

        gripAction = map.FindAction(gripName);
        triggerAction = map.FindAction(triggerName);
        primaryAction = map.FindAction(primaryName);
        secondaryAction = map.FindAction(secondaryName);

        gripAction.performed += OnGripChanged;
        gripAction.canceled += OnGripChanged;
        gripAction.Enable();

        triggerAction.performed += OnTriggerChanged;
        triggerAction.canceled += OnTriggerChanged;
        triggerAction.Enable();

        primaryAction.performed += OnPrimaryChanged;
        primaryAction.canceled += OnPrimaryChanged;
        primaryAction.Enable();

        secondaryAction.performed += OnSecondaryChanged;
        secondaryAction.canceled += OnSecondaryChanged;
        secondaryAction.Enable();
    }

    private void OnGripChanged(InputAction.CallbackContext context)
    {
        gripValue = context.ReadValue<float>();
    }
    private void OnTriggerChanged(InputAction.CallbackContext context)
    {
        triggerValue = context.ReadValue<float>();
    }
    private void OnPrimaryChanged(InputAction.CallbackContext context)
    {
        primaryValue = context.ReadValue<float>();
    }
    private void OnSecondaryChanged(InputAction.CallbackContext context)
    {
        secondaryValue = context.ReadValue<float>();
    }

    public float GetGrip()
    {
        return gripValue;
    }

    public float GetTrigger()
    {
        return triggerValue;
    }

    public float GetPrimary()
    {
        return primaryValue;
    }

    public float GetSecondary()
    {
        return secondaryValue;
    }
}
