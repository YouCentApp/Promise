public class NativeNavigationManager : INavigationManager
{
    public Task NavigateToAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }
}