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
    public long Cents { get; set; }
    public string? Memo { get; set; }
}

public class TransactionsHistory
{
    public User? User { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public bool IsOldFirst { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}