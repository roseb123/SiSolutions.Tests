#nullable enable

using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
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
        // Prefer matching-browser resolution to avoid unnecessary network lookups for latest.
        try
        {
            new DriverManager().SetUpDriver(new EdgeConfig(), VersionResolveStrategy.MatchingBrowser);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DriverManager.SetUpDriver failed: {ex.GetType().Name}: {ex.Message}");
            // Swallow here — we'll try to fall back to a local driver if available.
        }

        var options = new EdgeOptions();
        ApplyCommonArguments(options, settings);

        // Try creating the EdgeDriver normally. If the driver binary isn't available
        // (network or WebDriverManager failure), attempt to locate a local msedgedriver
        // on PATH and use it. If that also fails, rethrow to let the caller decide.
        try
        {
            var driver = new EdgeDriver(options);
            PostConfigure(driver);
            return driver;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"EdgeDriver creation failed: {ex.GetType().Name}: {ex.Message}");

            // Look for a local msedgedriver executable on PATH.
            var exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "msedgedriver.exe" : "msedgedriver";
            var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (var dir in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    var candidate = Path.Combine(dir, exeName);
                    if (File.Exists(candidate))
                    {
                        var service = EdgeDriverService.CreateDefaultService(dir, exeName);
                        var driver = new EdgeDriver(service, options);
                        PostConfigure(driver);
                        return driver;
                    }
                }
                catch (Exception innerEx)
                {
                    Debug.WriteLine($"Searching PATH for msedgedriver failed in '{dir}': {innerEx.GetType().Name}: {innerEx.Message}");
                    // ignore and continue searching
                }
            }

            // If we reach here, no driver could be created; rethrow to let caller handle it.
            throw;
        }
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
