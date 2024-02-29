namespace OtusSocialNetwork.DataClasses.Responses;

public class ErrorRes
{
    public ErrorRes(string message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public string Message { get; set; }
}
