namespace DeflexPro.OpenPave;

public sealed class OpenPaveException : Exception
{
    public string Operation { get; }
    public int Status { get; }

    public OpenPaveException(string operation, int status)
        : base($"OpenPave operation '{operation}' failed with status {status}.")
    {
        Operation = operation;
        Status = status;
    }
}
