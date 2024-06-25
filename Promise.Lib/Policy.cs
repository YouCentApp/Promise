public class Policy
{
    public const int MinimumPasswordLength = 8;

    public const int MaximumPasswordLength = 40;

    public const int MinimumUsernameLength = 5;

    public const int MaximumUsernameLength = 50;

    public const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,40}$";
}



