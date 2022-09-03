public class LeftInputs : Inputs
{
    public static LeftInputs Instance;

    private void Awake()
    {
        Instance = this;
        SetInputs();
    }
}
