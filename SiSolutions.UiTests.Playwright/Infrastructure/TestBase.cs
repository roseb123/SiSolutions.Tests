#nullable enable

using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;

namespace SiSolutions.UiTests.Playwright.Infrastructure;

public abstract class TestBase
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage _page = null!;

    protected IPage Page => _page;

    protected TestSettings Settings { get; private set; } = null!;

    protected float DefaultTimeoutMs => Settings.TimeoutSeconds * 1000f;

    [SetUp]
    public async Task BaseSetUp()
    {
        Settings = LoadSettings();
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await LaunchBrowserAsync(_playwright, Settings);

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 },
        });

        _page = await _context.NewPageAsync();
        _page.SetDefaultTimeout(DefaultTimeoutMs);

        await _page.GotoAsync(Settings.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
        });

        await _page.WaitForFunctionAsync("() => document.title && document.title.trim().length > 0", null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeoutMs });
    }

    [TearDown]
    public async Task BaseTearDown()
    {
        try
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed && _page is not null)
            {
                var artifactDir = Path.Combine(
                    TestContext.CurrentContext.TestDirectory,
                    "artifacts", "screenshots");

                Directory.CreateDirectory(artifactDir);

                var safeName = SanitiseFileName(TestContext.CurrentContext.Test.Name);
                var fileName = $"{safeName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
                var fullPath = Path.Combine(artifactDir, fileName);

                await _page.ScreenshotAsync(new PageScreenshotOptions { Path = fullPath, FullPage = true });
                TestContext.Out.WriteLine($"[SCREENSHOT] {fullPath}");
                TestContext.Out.WriteLine($"[FAILURE] URL   : {_page.Url}");
                TestContext.Out.WriteLine($"[FAILURE] Title : {await _page.TitleAsync()}");
            }
        }
        finally
        {
            if (_context is not null) await _context.CloseAsync();
            if (_browser is not null) await _browser.CloseAsync();
            _playwright?.Dispose();
        }
    }

    private static async Task<IBrowser> LaunchBrowserAsync(IPlaywright playwright, TestSettings settings)
    {
        var browserName = settings.Browser.Trim().ToUpperInvariant();

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = settings.Headless,
        };

        if (browserName == "CHROME")
            launchOptions.Channel = "chrome";
        else if (browserName == "EDGE")
            launchOptions.Channel = "msedge";

        return await playwright.Chromium.LaunchAsync(launchOptions);
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
            ?? throw new InvalidOperationException(
                "Failed to bind appsettings.json to TestSettings. " +
                "Ensure the file is present in the test output directory.");
    }
}
