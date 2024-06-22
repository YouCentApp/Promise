public interface IMyEnvironment
{
    bool IsDevelopment();
    bool IsProduction();
    string GetEnvironment();
}
