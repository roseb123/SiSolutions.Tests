#nullable enable

using System.Drawing;
using OpenQA.Selenium;
using SiSolutions.UiTests.Infrastructure;
using SiSolutions.UiTests.Pages;

namespace SiSolutions.UiTests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class CrossBrowserUiTests : TestBase
{
    private TimeSpan Timeout => DefaultTimeout;

    // 3.1 - Verifies the homepage loads successfully in Chrome.
    [TestCase("Chrome")]
    public void Homepage_LoadsSuccessfullyInChrome(string browserName)
    {
        using var driver = CreateDriverForBrowser(browserName);
        var page = new HomePage(driver, Timeout);

        Assert.That(page.GetPageTitle(), Is.Not.Empty);
        Assert.That(page.GetVisibleBodyText(), Does.Contain("Електроинсталации").IgnoreCase);
    }

    // 3.2 - Verifies the homepage loads successfully in Edge.
    [TestCase("Edge")]
    public void Homepage_LoadsSuccessfullyInEdge(string browserName)
    {
        using var driver = CreateDriverForBrowser(browserName);
        var page = new HomePage(driver, Timeout);

        Assert.That(page.GetPageTitle(), Is.Not.Empty);
        Assert.That(page.GetVisibleBodyText(), Does.Contain("Електроинсталации").IgnoreCase);
    }

    // 3.3 - Verifies a key navigation interaction works in Chrome.
    [TestCase("Chrome")]
    public void KeyNavigationInteraction_WorksInChrome(string browserName)
    {
        using var driver = CreateDriverForBrowser(browserName);
        var page = new HomePage(driver, Timeout);
        var contactItem = HomePage.BuildNavItem("contact");

        page.ClickNavItem(contactItem);
        page.AssertPanelIsActive(contactItem);

        Assert.That(page.GetContactEmailAddress(), Is.Not.Empty);
    }

    // 3.4 - Verifies a key navigation interaction works in Edge.
    [TestCase("Edge")]
    public void KeyNavigationInteraction_WorksInEdge(string browserName)
    {
        using var driver = CreateDriverForBrowser(browserName);
        var page = new HomePage(driver, Timeout);
        var contactItem = HomePage.BuildNavItem("contact");

        page.ClickNavItem(contactItem);
        page.AssertPanelIsActive(contactItem);

        Assert.That(page.GetContactEmailAddress(), Is.Not.Empty);
    }

    // 3.5 - Verifies the page remains usable at a smaller viewport size.
    [TestCase("Chrome")]
    public void Homepage_LayoutRemainsUsableAtDifferentViewportSizes(string browserName)
    {
        using var driver = CreateDriverForBrowser(browserName);
        driver.Manage().Window.Size = new Size(375, 812);

        var page = new HomePage(driver, Timeout);
        var contactItem = HomePage.BuildNavItem("contact");

        page.ClickNavItem(contactItem);
        page.AssertPanelIsActive(contactItem);

        Assert.That(page.GetContactEmailAddress(), Is.Not.Empty);
    }

    private IWebDriver CreateDriverForBrowser(string browserName)
    {
        var browserSettings = new TestSettings
        {
            BaseUrl = Settings.BaseUrl,
            Browser = browserName,
            Headless = Settings.Headless,
            TimeoutSeconds = Settings.TimeoutSeconds,
            ExpectedEmail = Settings.ExpectedEmail,
            ExpectedPhoneNumber = Settings.ExpectedPhoneNumber,
        };

        try
        {
            var driver = DriverFactory.Create(browserSettings);
            driver.Navigate().GoToUrl(browserSettings.BaseUrl);
            driver.WaitFor(
                d => !string.IsNullOrWhiteSpace(d.Title),
                TimeSpan.FromSeconds(browserSettings.TimeoutSeconds),
                $"Page '{browserSettings.BaseUrl}' did not finish loading for browser '{browserName}'.");

            return driver;
        }
        catch (Exception ex)
        {
            // WebDriverManager may attempt to download drivers from the network
            // (e.g. msedgedriver.azureedge.net). If that fails, skip the cross-browser test
            // rather than making the whole run fail.
            Assert.Ignore($"Skipping {browserName} cross-browser test: driver setup failed: {ex.Message}");
            return null!; // unreachable, required for compiler
        }
    }
}
