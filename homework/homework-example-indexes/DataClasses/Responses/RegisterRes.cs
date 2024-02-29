namespace OtusSocialNetwork.DataClasses.Responses;

public class RegisterRes
{
    public RegisterRes(string userId)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
    }

    public string UserId { get; set; }
}
