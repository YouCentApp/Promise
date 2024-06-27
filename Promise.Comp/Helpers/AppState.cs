public class AppState
{
    public bool IsSignedIn { get; set; }

    public long UserId { get; set; }

    public string? Username { get; set; }

    public string? Token { get; set; }

    public void Clean()
    {
        IsSignedIn = false;
        UserId = 0;
        Username = null;
        Token = null;
    }
}
