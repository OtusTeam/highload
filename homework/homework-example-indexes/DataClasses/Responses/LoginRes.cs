namespace OtusSocialNetwork.DataClasses.Responses;

public class LoginRes
{
    public LoginRes(string token)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
    }

    public string Token { get; set; }
}
