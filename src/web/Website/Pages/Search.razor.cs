//-----------------------------------------------------------------------
// <copyright file="Search.razor.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Search Code-Behind
// </summary>
//-----------------------------------------------------------------------
using Microsoft.AspNetCore.Components.Authorization;

namespace GetTogether.Web.Pages;

/// <summary>
/// Search Page
/// </summary>
public partial class Search : ComponentBase
{
    [Inject] IServiceProvider ServiceProvider { get; set; }
    [Inject] IJSRuntime JsInterop { get; set; }
    [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] ICurrentUserResolver CurrentUserResolver { get; set; }

    private string SearchTerm = string.Empty;
    private int? SelectedCircleId = null;
    private List<Circle> userCircles = new();
    private List<EventSearchResult> matchingEvents = new();
    private bool hasSearched = false;
    private User currentUser = null;

    private class EventSearchResult
    {
        public int EventId { get; set; }
        public int CircleId { get; set; }
        public string CircleName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartUtc { get; set; }
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
            await LoadUserCirclesAsync();
            await JsInterop.InvokeVoidAsync("focusOnInputField", "inputText");
            StateHasChanged();
        }
    }

    private async Task LoadUserCirclesAsync()
    {
        using var scope = ServiceProvider.CreateScope();
        var circleRepo = scope.ServiceProvider.GetService<ICircleRepository>();

        if (circleRepo == null) return;

        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var resolution = await CurrentUserResolver.ResolveAsync(authState.User);
        currentUser = resolution.User;
        if (currentUser == null) return;

        userCircles = (await circleRepo.GetCirclesForUserAsync(currentUser.UserId)).ToList();
    }

    private async Task ExecuteSearch()
    {
        hasSearched = true;
        matchingEvents.Clear();

        await JsInterop.InvokeVoidAsync("focusOnInputField", "btnSearch");

        using var scope = ServiceProvider.CreateScope();
        var eventRepo = scope.ServiceProvider.GetService<IEventRepository>();
        var circleRepo = scope.ServiceProvider.GetService<ICircleRepository>();

        if (eventRepo == null || circleRepo == null || currentUser == null) return;

        var circlesToSearch = SelectedCircleId.HasValue
            ? userCircles.Where(c => c.CircleId == SelectedCircleId.Value).ToList()
            : userCircles;

        foreach (var circle in circlesToSearch)
        {
            var events = await eventRepo.GetEventsForCircleAsync(circle.CircleId, currentUser.UserId);
            var filtered = events.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filtered = filtered.Where(e =>
                    (e.Title ?? "").Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (e.Details ?? "").Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var evt in filtered)
            {
                matchingEvents.Add(new EventSearchResult
                {
                    EventId = evt.EventId,
                    CircleId = evt.CircleId,
                    CircleName = circle.Name,
                    Title = evt.Title,
                    Description = evt.Details ?? string.Empty,
                    Location = string.Empty,
                    StartUtc = evt.StartsUtc
                });
            }
        }

        matchingEvents = matchingEvents.OrderBy(e => e.StartUtc).ToList();
        StateHasChanged();
    }

    private void CheckForEnterKey(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            _ = ExecuteSearch();
        }
    }
}
