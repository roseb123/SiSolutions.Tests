#nullable enable

using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;

namespace SiSolutions.UiTests.Playwright.Infrastructure;

public abstract class TestBase
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    protected IPage Page => _page;

    protected TestSettings Settings { get; private set; } = null!;

    protected TimeSpan DefaultTimeout => TimeSpan.FromSeconds(Settings.TimeoutSeconds);

    [SetUp]
    public async Task BaseSetUp()
    {
        Settings = LoadSettings();
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        var browserChannel = Settings.Browser.Equals("Edge", StringComparison.OrdinalIgnoreCase)
            ? "msedge"
            : "chrome";

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Channel = browserChannel,
            Headless = Settings.Headless,
        });

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1440, Height = 960 },
        });

        _page = await _context.NewPageAsync();
        _page.SetDefaultTimeout((float)DefaultTimeout.TotalMilliseconds);

        await _page.GotoAsync(Settings.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = (float)DefaultTimeout.TotalMilliseconds,
        });

        await _page.WaitForPageReadyAsync(DefaultTimeout);
    }

    [TearDown]
    public async Task BaseTearDown()
    {
        try
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed && _page is not null)
            {
                await CaptureScreenshotAsync();
                TestContext.Out.WriteLine($"[FAILURE] URL   : {_page.Url}");
                TestContext.Out.WriteLine($"[FAILURE] Title : {await _page.TitleAsync()}");
            }
        }
        finally
        {
            if (_context is not null)
            {
                await _context.CloseAsync();
            }

            if (_browser is not null)
            {
                await _browser.CloseAsync();
            }

            _playwright?.Dispose();
        }
    }

    private async Task CaptureScreenshotAsync()
    {
        try
        {
            var artifactDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "artifacts", "screenshots");
            Directory.CreateDirectory(artifactDir);

            var safeName = SanitiseFileName(TestContext.CurrentContext.Test.Name);
            var fileName = $"{safeName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
            var fullPath = Path.Combine(artifactDir, fileName);

            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = fullPath,
                FullPage = true,
            });

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

    private static TestSettings LoadSettings()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        return config.Get<TestSettings>()
            ?? throw new InvalidOperationException("Failed to bind appsettings.json to TestSettings. Ensure the file is present in the test output directory.");
    }
}