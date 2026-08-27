using GetTogether.Web.Pages;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GetTogether.Tests;

public class GoogleAuthentication_Tests
{
    [Fact]
    public async Task Login_WhenGoogleCredentialsAreConfigured_RendersGoogleChallengeLink()
    {
        var markup = await RenderLoginAsync(new Dictionary<string, string?>
        {
            ["Authentication:Google:ClientId"] = "google-client-id",
            ["Authentication:Google:ClientSecret"] = "google-client-secret"
        });

        Assert.Contains("href=\"/login/google\"", markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null, "google-client-secret")]
    [InlineData("", "google-client-secret")]
    [InlineData("google-client-id", null)]
    [InlineData("google-client-id", "")]
    public async Task Login_WhenGoogleCredentialsAreMissingOrBlank_RendersDisabledGoogleEntry(string? clientId, string? clientSecret)
    {
        var markup = await RenderLoginAsync(new Dictionary<string, string?>
        {
            ["Authentication:Google:ClientId"] = clientId,
            ["Authentication:Google:ClientSecret"] = clientSecret
        });

        Assert.Contains("provider-button provider-google", markup, StringComparison.Ordinal);
        Assert.Contains("disabled", markup, StringComparison.Ordinal);
        Assert.Contains("Not configured", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"/login/google\"", markup, StringComparison.Ordinal);
    }

    private static async Task<string> RenderLoginAsync(IReadOnlyDictionary<string, string?> settings)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());

        await using var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        await using var renderer = new HtmlRenderer(serviceProvider, loggerFactory);

        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var component = await renderer.RenderComponentAsync<Login>();
            return component.ToHtmlString();
        });
    }
}