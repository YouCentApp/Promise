public class AppState
{
    public bool IsSignedIn { get; set; }

    public bool IsSignedInChecking { get; set; }

    public bool IsSignedInChecked { get; set; }

    public long UserId { get; set; }

    public string? Username { get; set; }

    public string? Token { get; set; }

    public void Clean()
    {
        IsSignedIn = false;
        IsSignedInChecking = false;
        IsSignedInChecked = false;
        UserId = 0;
        Username = null;
        Token = null;
    }
}
