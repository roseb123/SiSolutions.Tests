#nullable enable

using Microsoft.Extensions.Configuration;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;

namespace SiSolutions.UiTests.Infrastructure;

/// <summary>
/// Base class for all UI test fixtures.
/// Responsibilities:
///   - Load settings from appsettings.json
///   - Create and configure the WebDriver before each test
///   - Quit the driver after each test (always)
///   - Capture a screenshot and log diagnostic info on failure
/// </summary>
public abstract class TestBase
{
    private IWebDriver _driver = null!;

    // ── Accessible to derived classes ───────────────────────────────────────

    protected IWebDriver Driver => _driver;

    protected TestSettings Settings { get; private set; } = null!;

    protected TimeSpan DefaultTimeout => TimeSpan.FromSeconds(Settings.TimeoutSeconds);

    // ── NUnit lifecycle ─────────────────────────────────────────────────────

    [SetUp]
    public void BaseSetUp()
    {
        Settings = LoadSettings();
        _driver  = DriverFactory.Create(Settings);
        _driver.Navigate().GoToUrl(Settings.BaseUrl);

        // Wait until the page has a title — acts as a basic page-load confirmation.
        _driver.WaitFor(
            d => !string.IsNullOrWhiteSpace(d.Title),
            DefaultTimeout,
            $"Page '{Settings.BaseUrl}' did not finish loading (empty title).");
    }

    [TearDown]
    public void BaseTearDown()
    {
        try
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                CaptureScreenshot();
                TestContext.Out.WriteLine($"[FAILURE] URL   : {_driver.Url}");
                TestContext.Out.WriteLine($"[FAILURE] Title : {_driver.Title}");
            }
        }
        finally
        {
            // Dispose() calls Quit() internally and satisfies NUnit1032.
            _driver?.Dispose();
        }
    }

    // ── Screenshot helper ───────────────────────────────────────────────────

    private void CaptureScreenshot()
    {
        if (_driver is not ITakesScreenshot shooter)
            return;

        try
        {
            var artifactDir = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "artifacts", "screenshots");

            Directory.CreateDirectory(artifactDir);

            // Sanitise the test name so it is safe as a filename on all platforms.
            var safeName = SanitiseFileName(TestContext.CurrentContext.Test.Name);
            var fileName = $"{safeName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
            var fullPath = Path.Combine(artifactDir, fileName);

            shooter.GetScreenshot().SaveAsFile(fullPath);
            TestContext.Out.WriteLine($"[SCREENSHOT] {fullPath}");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"[SCREENSHOT] Capture failed: {ex.Message}");
        }
    }

    private static string SanitiseFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }

    // ── Settings loader ─────────────────────────────────────────────────────

    private static TestSettings LoadSettings()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        return config.Get<TestSettings>()
            ?? throw new InvalidOperationException(
                "Failed to bind appsettings.json to TestSettings. " +
                "Ensure the file is present in the test output directory.");
    }
}
