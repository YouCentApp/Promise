namespace Promise.Api;

using System;
using Microsoft.EntityFrameworkCore;

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

public class YCDBContext : DbContext
{
    public YCDBContext(DbContextOptions<YCDBContext> options) : base(options) { }

    public DbSet<Currency> Currencies { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Balance> Balances { get; set; }
    public DbSet<PromiseLimit> PromiseLimits { get; set; }
    public DbSet<PromiseTransaction> PromiseTransactions { get; set; }
    public DbSet<Rate> Rates { get; set; }
    public DbSet<UserSetting> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Currency>().HasKey(c => c.Id);
        modelBuilder.Entity<Language>().HasKey(l => l.Id);
        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<Balance>().HasKey(b => b.UserId);
        modelBuilder.Entity<PromiseLimit>().HasKey(pl => pl.UserId);
        modelBuilder.Entity<PromiseTransaction>().HasKey(pt => pt.Id);
        modelBuilder.Entity<Rate>().HasKey(r => r.CurrencyId);
        modelBuilder.Entity<UserSetting>().HasKey(us => us.UserId);

        modelBuilder.Entity<PromiseLimit>()
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<PromiseLimit>(pl => pl.UserId);

        modelBuilder.Entity<Balance>()
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<Balance>(b => b.UserId);

        modelBuilder.Entity<PromiseTransaction>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(pt => pt.SenderId);

        modelBuilder.Entity<PromiseTransaction>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(pt => pt.ReceiverId);

        modelBuilder.Entity<UserSetting>()
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<UserSetting>(us => us.UserId);

        modelBuilder.Entity<UserSetting>()
            .HasOne<Language>()
            .WithMany()
            .HasForeignKey(us => us.LanguageId);

        modelBuilder.Entity<UserSetting>()
            .HasOne<Currency>()
            .WithMany()
            .HasForeignKey(us => us.CurrencyId);

        modelBuilder.Entity<Rate>()
            .HasOne<Currency>()
            .WithMany()
            .HasForeignKey(r => r.CurrencyId);
    }
}