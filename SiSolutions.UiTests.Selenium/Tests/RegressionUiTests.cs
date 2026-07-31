#nullable enable

using SiSolutions.UiTests.Infrastructure;
using SiSolutions.UiTests.Pages;

namespace SiSolutions.UiTests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class RegressionUiTests : TestBase
{
    private HomePage _homePage = null!;

    [SetUp]
    public void SetUpPage()
    {
        _homePage = new HomePage(Driver, DefaultTimeout);
    }

    // 2.1 - Verifies the Electrical section remains accessible and shows its heading.
    [Test]
    public void ElectricalSection_HeadingIsVisibleWhenNavigatedTo()
    {
        var electricalItem = HomePage.BuildNavItem("electrical");

        _homePage.ClickNavItem(electricalItem);
        _homePage.AssertPanelIsActive(electricalItem);

        var headingText = _homePage.GetSectionHeading("electrical");

        Assert.That(headingText, Is.Not.Empty,
            "The electrical section heading should be visible after navigation.");
    }

    // 2.2 - Verifies the Contact section is still accessible after navigating away and back.
    [Test]
    public void ContactSection_StillAccessibleAfterReturningFromAnotherSection()
    {
        var electricalItem = HomePage.BuildNavItem("electrical");
        var contactItem = HomePage.BuildNavItem("contact");

        _homePage.ClickNavItem(electricalItem);
        _homePage.AssertPanelIsActive(electricalItem);

        _homePage.ClickNavItem(contactItem);
        _homePage.AssertPanelIsActive(contactItem);

        Assert.That(_homePage.GetContactEmailAddress(), Is.Not.Empty);
    }

    // 2.3 - Verifies navigation updates the URL hash correctly for the active section.
    [Test]
    public void Homepage_MaintainsExpectedUrlHashOnNavigation()
    {
        var contactItem = HomePage.BuildNavItem("contact");

        _homePage.ClickNavItem(contactItem);
        _homePage.AssertPanelIsActive(contactItem);

        Assert.That(Driver.Url, Does.EndWith("#contact"));
    }

    // 2.4 - Verifies key navigation links resolve to real sections rather than broken or empty content.
    [Test]
    public void NavigationLinks_DoNotLeadToBrokenOrEmptySections()
    {
        foreach (var sectionId in HomePage.SectionIds)
        {
            var item = HomePage.BuildNavItem(sectionId);
            _homePage.ClickNavItem(item);
            _homePage.AssertPanelIsActive(item);

            var headingText = _homePage.GetSectionHeading(sectionId);
            Assert.That(headingText, Is.Not.Empty,
                $"Section '{sectionId}' did not render a visible heading after navigation.");
        }
    }

    // 2.5 - Verifies a critical user journey can reach the contact information end to end.
    [Test]
    public void CriticalUserJourney_CanReachContactInformation()
    {
        var electricalItem = HomePage.BuildNavItem("electrical");
        var contactItem = HomePage.BuildNavItem("contact");

        _homePage.ClickNavItem(electricalItem);
        _homePage.AssertPanelIsActive(electricalItem);

        _homePage.ClickNavItem(contactItem);
        _homePage.AssertPanelIsActive(contactItem);

        Assert.That(_homePage.GetContactEmailAddress(), Is.Not.Empty);
        Assert.That(_homePage.GetContactPhoneNumber(), Is.Not.Empty);
    }
}
