#nullable enable

using SiSolutions.UiTests.Playwright.Infrastructure;
using SiSolutions.UiTests.Playwright.Pages;

namespace SiSolutions.UiTests.Playwright.Tests;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class ContactInfoTests : TestBase
{
    private HomePage _homePage = null!;

    [SetUp]
    public void SetUpPage()
    {
        _homePage = new HomePage(Page, DefaultTimeoutMs);
    }

    [Test]
    public async Task ContactSection_DisplaysConfiguredEmailAddress()
    {
        var contactItem = HomePage.BuildNavItem("contact");
        var expectedEmail = Settings.ExpectedEmail;

        await _homePage.ClickNavItem(contactItem);
        await _homePage.AssertPanelIsActive(contactItem);

        var actualEmail = await _homePage.GetContactEmailAddress();

        Assert.That(
            actualEmail,
            Is.EqualTo(expectedEmail).IgnoreCase,
            $"Contact email displayed on site ('{actualEmail}') does not match " +
            $"the expected value ('{expectedEmail}') configured in appsettings.json. " +
            $"URL={Page.Url} | Title={await Page.TitleAsync()}");
    }

    [Test]
    public async Task CheckPhoneNumberIsAccurate()
    {
        var contactItem = HomePage.BuildNavItem("contact");
        var expectedPhoneNumber = Settings.ExpectedPhoneNumber;
        var localFormat = expectedPhoneNumber.TrimStart('+').TrimStart('3', '5', '9');
        var intlFormat = "+359" + localFormat.TrimStart('0');

        await _homePage.ClickNavItem(contactItem);
        await _homePage.AssertPanelIsActive(contactItem);

        var actualPhone = await _homePage.GetContactPhoneNumber();

        Assert.That(
            actualPhone,
            Is.EqualTo(expectedPhoneNumber).Or.EqualTo(localFormat).Or.EqualTo(intlFormat),
            $"Contact phone displayed on site ('{actualPhone}') does not match " +
            $"the expected value ('{expectedPhoneNumber}') configured in appsettings.json. " +
            $"URL={Page.Url} | Title={await Page.TitleAsync()}");
    }
}
