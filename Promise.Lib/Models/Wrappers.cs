public class UserData
{
    public User? User { get; set; }
    public PersonalData? PersonalData { get; set; }
}

public class UserUpdate
{
    public User? OldUser { get; set; }
    public User? NewUser { get; set; }
}

public class UserTransaction
{
    public User? Sender { get; set; }
    public User? Receiver { get; set; }
    public int Cents { get; set; }
    public string? Memo { get; set; }
}