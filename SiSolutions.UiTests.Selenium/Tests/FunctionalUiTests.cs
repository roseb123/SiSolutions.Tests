#nullable enable

using System.Linq;
using SiSolutions.UiTests.Infrastructure;
using SiSolutions.UiTests.Pages;

namespace SiSolutions.UiTests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class FunctionalUiTests : TestBase
{
    private HomePage _homePage = null!;

    [SetUp]
    public void SetUpPage()
    {
        _homePage = new HomePage(Driver, DefaultTimeout);
    }

    // 1.1 - Verifies the homepage loads and exposes a non-empty title.
    [Test]
    public void Homepage_LoadsAndDisplaysExpectedTitle()
    {
        var title = _homePage.GetPageTitle();

        Assert.That(title, Is.Not.Empty, "The page title should not be empty.");

        var acceptedPhrases = new[] { "SiSolutions", "ЕС АЙ СОЛЮШЪНС" };

        Assert.That(
            acceptedPhrases.Any(p => title.Contains(p, StringComparison.OrdinalIgnoreCase)),
            $"Unexpected page title '{title}'. Expected one of: {string.Join(", ", acceptedPhrases)}");
    }

    // 1.2 - Verifies the homepage contains the main visible service content.
    [Test]
    public void Homepage_ContainsMainServiceContent()
    {
        var bodyText = _homePage.GetVisibleBodyText();

        Assert.That(bodyText, Does.Contain("Електроинсталации").IgnoreCase,
            "The page should contain the main service content.");
    }

    // 1.3 - Verifies the navigation menu exposes the expected sections.
    [Test]
    public void NavigationMenu_ContainsAllExpectedSections()
    {
        var sectionIds = _homePage.GetNavSectionIds();

        Assert.That(sectionIds, Is.Not.Empty);
        Assert.That(sectionIds, Does.Contain("electrical"));
        Assert.That(sectionIds, Does.Contain("contact"));
        Assert.That(sectionIds, Does.Contain("why-us"));
    }

    // 1.4 - Verifies the Contact section becomes active and shows its heading.
    [Test]
    public void ContactSection_HeadingIsVisibleWhenNavigatedTo()
    {
        var contactItem = HomePage.BuildNavItem("contact");

        _homePage.ClickNavItem(contactItem);
        _homePage.AssertPanelIsActive(contactItem);

        var headingText = _homePage.GetSectionHeading("contact");

        Assert.That(headingText, Is.Not.Empty,
            "The contact section heading should be visible after navigation.");
    }

    // 1.5 - Verifies the Why Us section becomes active and shows its heading.
    [Test]
    public void WhyUsSection_HeadingIsVisibleWhenNavigatedTo()
    {
        var whyUsItem = HomePage.BuildNavItem("why-us");

        _homePage.ClickNavItem(whyUsItem);
        _homePage.AssertPanelIsActive(whyUsItem);

        var headingText = _homePage.GetSectionHeading("why-us");

        Assert.That(headingText, Is.Not.Empty,
            "The why-us section heading should be visible after navigation.");
    }
}
