//-----------------------------------------------------------------------
// <copyright file="Index.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Index Page Code Behind
// </summary>
//-----------------------------------------------------------------------
using Microsoft.AspNetCore.Authorization;

namespace GetTogether.Web.Pages;

/// <summary>
/// Index Page Code Behind
/// </summary>
[AllowAnonymous]
public partial class Index : ComponentBase, IDisposable
{
    [Inject] IJSRuntime JsInterop { get; set; }
    [Inject] GetTogether.Web.Repositories.ThemeService ThemeService { get; set; }

    private bool isNinetiesTheme = false;

    /// <summary>
    /// Initializes the component and subscribes to theme change events.
    /// </summary>
    protected override void OnInitialized()
    {
        ThemeService.OnThemeChanged += HandleThemeChanged;
    }

    /// <summary>
    /// Initialization
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await JsInterop.InvokeVoidAsync("syncHeaderTitle");
            var theme = await JsInterop.InvokeAsync<string>("localStorage.getItem", "theme-mode");

            // Validate theme value
            var validModes = new[] { "light", "dark", "nineties", "system" };
            if (!string.IsNullOrEmpty(theme) && !validModes.Contains(theme))
            {
                Console.WriteLine($"Warning: Invalid theme mode '{theme}' in localStorage, clearing.");
                await JsInterop.InvokeVoidAsync("localStorage.removeItem", "theme-mode");
                theme = null;
            }

            isNinetiesTheme = theme == "nineties";
            StateHasChanged();
        }
    }

    private async void HandleThemeChanged()
    {
        var theme = await JsInterop.InvokeAsync<string>("localStorage.getItem", "theme-mode");

        // Validate theme value
        var validModes = new[] { "light", "dark", "nineties", "system" };
        if (!string.IsNullOrEmpty(theme) && !validModes.Contains(theme))
        {
            Console.WriteLine($"Warning: Invalid theme mode '{theme}' in localStorage, clearing.");
            await JsInterop.InvokeVoidAsync("localStorage.removeItem", "theme-mode");
            theme = null;
        }

        isNinetiesTheme = theme == "nineties";
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Unsubscribes from theme change events when the component is disposed.
    /// </summary>
    public void Dispose()
    {
        ThemeService.OnThemeChanged -= HandleThemeChanged;
    }
}
