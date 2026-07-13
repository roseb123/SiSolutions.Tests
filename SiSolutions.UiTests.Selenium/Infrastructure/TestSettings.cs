#nullable enable

namespace SiSolutions.UiTests.Infrastructure;

/// <summary>
/// Configuration settings loaded from appsettings.json.
/// All defaults map to headless Chrome against the production URL.
/// </summary>
public sealed class TestSettings
{
    public string BaseUrl { get; init; } = "https://si-solutions-bg.vercel.app";

    /// <summary>Chrome or Edge (case-insensitive).</summary>
    public string Browser { get; init; } = "Chrome";

    /// <summary>Run browser headless when true (default: true).</summary>
    public bool Headless { get; init; } = true;

    /// <summary>Default explicit-wait timeout in seconds.</summary>
    public int TimeoutSeconds { get; init; } = 15;

    /// <summary>
    /// The email address shown in the contact section of the site.
    /// Override in appsettings.json to test a different address without recompiling.
    /// </summary>
    public string ExpectedEmail { get; init; } = "sisolutionsbg@gmail.com";

    /// <summary>
    /// The phone number shown in the contact section of the site.
    /// Accepted formats: "0876703085" or "+359876703085".
    /// Override in appsettings.json to test a different number without recompiling.
    /// </summary>
    public string ExpectedPhoneNumber { get; init; } = "0876703085";
}
