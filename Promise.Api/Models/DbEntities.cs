namespace Promise.Api;

public class Currency
{
    public byte Id { get; set; }
    public string? Code { get; set; }
    public string? Number { get; set; }
    public string? Name { get; set; }
}

public class Language
{
    public int Id { get; set; }
    public string? NameEng { get; set; }
    public string? NameCode { get; set; }
    public string? NameNative { get; set; }
}


public class User
{
    public long Id { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? Salt { get; set; }
    public DateTime CreationDate { get; set; }
}

public class Balance
{
    public long UserId { get; set; }
    public long Cents { get; set; }
}


public class PromiseLimit
{
    public long UserId { get; set; }
    public long Cents { get; set; }
}

public class PromiseTransaction
{
    public long Id { get; set; }
    public long SenderId { get; set; }
    public long ReceiverId { get; set; }
    public int Cents { get; set; }
    public DateTime Date { get; set; }
    public string? Hash { get; set; }
    public bool IsBlockchain { get; set; }
    public string? Memo { get; set; }
}

public class Rate
{
    public byte CurrencyId { get; set; }
    public float AmountFor100 { get; set; }
    public DateTime UpdateDate { get; set; }
}

public class UserSetting
{
    public long UserId { get; set; }
    public int LanguageId { get; set; }
    public byte CurrencyId { get; set; }
    public bool IsDarkTheme { get; set; }
}

