namespace CampusTrade.Backend.Services;

public class AuthException : Exception
{
    public int Code { get; }

    public AuthException(int code, string message) : base(message)
    {
        Code = code;
    }
}
