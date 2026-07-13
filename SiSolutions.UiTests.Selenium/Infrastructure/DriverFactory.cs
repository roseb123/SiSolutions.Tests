#nullable enable

using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Edge;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace SiSolutions.UiTests.Infrastructure;

/// <summary>
/// Creates a configured <see cref="IWebDriver"/> instance.
/// Supports Chrome (default) and Edge; switch via TestSettings.Browser.
/// Uses WebDriverManager to resolve the correct driver binary automatically.
/// </summary>
public static class DriverFactory
{
    private const int ViewportWidth  = 1280;
    private const int ViewportHeight = 800;

    public static IWebDriver Create(TestSettings settings)
    {
        return settings.Browser.Trim().ToUpperInvariant() switch
        {
            "EDGE" => CreateEdgeDriver(settings),
            _      => CreateChromeDriver(settings),
        };
    }

    // ── Chrome ─────────────────────────────────────────────────────────────

    private static IWebDriver CreateChromeDriver(TestSettings settings)
    {
        new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);

        var options = new ChromeOptions();
        ApplyCommonArguments(options, settings);

        var driver = new ChromeDriver(options);
        PostConfigure(driver);
        return driver;
    }

    // ── Edge ───────────────────────────────────────────────────────────────

    private static IWebDriver CreateEdgeDriver(TestSettings settings)
    {
        new DriverManager().SetUpDriver(new EdgeConfig());

        var options = new EdgeOptions();
        ApplyCommonArguments(options, settings);

        var driver = new EdgeDriver(options);
        PostConfigure(driver);
        return driver;
    }

    // ── Shared helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Applies identical Chromium arguments to both Chrome and Edge options.
    /// Both option types derive from <see cref="ChromiumOptions"/>.
    /// </summary>
    private static void ApplyCommonArguments(ChromiumOptions options, TestSettings settings)
    {
        if (settings.Headless)
            options.AddArgument("--headless=new");

        options.AddArguments(
            $"--window-size={ViewportWidth},{ViewportHeight}",
            "--no-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu",
            "--disable-extensions",
            "--disable-infobars"
        );
    }

    /// <summary>
    /// Post-creation driver configuration applied once, regardless of browser type.
    /// Explicit waits only — implicit wait is explicitly zeroed out.
    /// </summary>
    private static void PostConfigure(IWebDriver driver)
    {
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        driver.Manage().Window.Size = new Size(ViewportWidth, ViewportHeight);
    }
}
