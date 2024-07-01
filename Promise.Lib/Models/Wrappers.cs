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