#nullable enable

using SiSolutions.UiTests.Playwright.Infrastructure;
using SiSolutions.UiTests.Playwright.Pages;

namespace SiSolutions.UiTests.Playwright.Tests;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public sealed class NavigationMenuTests : TestBase
{
    private HomePage _homePage = null!;

    private static IEnumerable<TestCaseData> NavItemTestCases()
        => HomePage.SectionIds.Select(id =>
            new TestCaseData(HomePage.BuildNavItem(id))
                .SetName($"Nav_{id}"));

    [SetUp]
    public void SetUpPage()
    {
        _homePage = new HomePage(Page, DefaultTimeoutMs);
    }

    [Test, TestCaseSource(nameof(NavItemTestCases))]
    public async Task WhenNavItemClicked_CorrectPanelBecomesActive(NavItem item)
    {
        await _homePage.ClickNavItem(item);
        await _homePage.AssertPanelIsActive(item);
    }
}
