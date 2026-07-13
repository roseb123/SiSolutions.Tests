#nullable enable

using SiSolutions.UiTests.Infrastructure;
using SiSolutions.UiTests.Pages;

namespace SiSolutions.UiTests.Tests;

/// <summary>
/// Verifies that the contact information displayed on
/// https://si-solutions-bg.vercel.app matches the values configured in appsettings.json.
///
/// To test a different email address without recompiling, update
/// <c>"ExpectedEmail"</c> in <c>appsettings.json</c> (or any environment-specific
/// override file) and re-run <c>dotnet test</c>.
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class ContactInfoTests : TestBase
{
    private HomePage _homePage = null!;

    [SetUp]
    public void SetUpPage()
    {
        _homePage = new HomePage(Driver, DefaultTimeout);
    }

    /// <summary>
    /// Navigates to the Contact section, waits for the panel to become active,
    /// reads the displayed email link, and asserts it matches <see cref="TestSettings.ExpectedEmail"/>.
    ///
    /// Two things are verified:
    /// <list type="number">
    ///   <item>The contact panel is reachable via the nav link (delegates to <c>AssertPanelIsActive</c>).</item>
    ///   <item>The email address visible to the user matches the configured expected value.</item>
    /// </list>
    /// </summary>
    [Test]
    public void ContactSection_DisplaysConfiguredEmailAddress()
    {
        // ── Arrange ───────────────────────────────────────────────────────────
        var contactItem   = HomePage.BuildNavItem("contact");
        var expectedEmail = Settings.ExpectedEmail;

        // ── Act ───────────────────────────────────────────────────────────────
        // Navigate to the contact section so the panel becomes visible.
        // AssertPanelIsActive also covers: CSS class, URL hash, and heading presence.
        _homePage.ClickNavItem(contactItem);
        _homePage.AssertPanelIsActive(contactItem);

        var actualEmail = _homePage.GetContactEmailAddress();

        // ── Assert ────────────────────────────────────────────────────────────
        Assert.That(
            actualEmail,
            Is.EqualTo(expectedEmail).IgnoreCase,
            $"Contact email displayed on site ('{actualEmail}') does not match " +
            $"the expected value ('{expectedEmail}') configured in appsettings.json. " +
            $"URL={Driver.Url} | Title={Driver.Title}");
    }

    /// <summary>
    /// Verifies that the phone number in the contact section matches the value in
    /// appsettings.json. Both the local format ("0876703085") and the international
    /// format ("+359876703085") are accepted.
    /// </summary>
    [Test]
    public void CheckPhoneNumberIsAccurate()
    {
        // ── Arrange ───────────────────────────────────────────────────────────
        var contactItem         = HomePage.BuildNavItem("contact");
        var expectedPhoneNumber = Settings.ExpectedPhoneNumber;
        var localFormat         = expectedPhoneNumber.TrimStart('+').TrimStart('3', '5', '9');
        var intlFormat          = "+359" + localFormat.TrimStart('0');

        // ── Act ───────────────────────────────────────────────────────────────
        _homePage.ClickNavItem(contactItem);
        _homePage.AssertPanelIsActive(contactItem);

        var actualPhone = _homePage.GetContactPhoneNumber();

        // ── Assert ────────────────────────────────────────────────────────────
        Assert.That(
            actualPhone,
            Is.EqualTo(expectedPhoneNumber).Or.EqualTo(localFormat).Or.EqualTo(intlFormat),
            $"Contact phone displayed on site ('{actualPhone}') does not match " +
            $"the expected value ('{expectedPhoneNumber}') configured in appsettings.json. " +
            $"URL={Driver.Url} | Title={Driver.Title}");
    }
}
