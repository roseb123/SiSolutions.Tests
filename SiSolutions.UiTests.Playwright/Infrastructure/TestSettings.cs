#nullable enable

namespace SiSolutions.UiTests.Playwright.Infrastructure;

public sealed class TestSettings
{
    public string BaseUrl { get; init; } = "https://si-solutions-bg.vercel.app";

    public string Browser { get; init; } = "Chrome";

    public bool Headless { get; init; } = true;

    public int TimeoutSeconds { get; init; } = 15;

    public string ExpectedEmail { get; init; } = "sisolutionsbg@gmail.com";

    public string ExpectedPhoneNumber { get; init; } = "0876703085";
}