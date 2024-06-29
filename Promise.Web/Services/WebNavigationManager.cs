
using Microsoft.AspNetCore.Components;

public class WebNavigationManager : INavigationManager
{
    private readonly NavigationManager _navigationManager;

    public WebNavigationManager(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public Task NavigateToAsync(string route)
    {
        _navigationManager.NavigateTo(route);
        return Task.CompletedTask;
    }
}