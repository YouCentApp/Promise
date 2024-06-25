public class SessionState
{
    public long UserId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? AccessToken { get; set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);
}