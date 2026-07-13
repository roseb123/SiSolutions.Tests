#nullable enable

using SiSolutions.UiTests.Infrastructure;
using SiSolutions.UiTests.Pages;

namespace SiSolutions.UiTests.Tests;

/// <summary>
/// Verifies that every top navigation menu item on
/// https://si-solutions-bg.vercel.app correctly activates its target content panel.
///
/// One NUnit test case is generated per nav item so failures are reported
/// independently — a broken "UPS" link does not hide a passing "Contact" link.
///
/// Test flow (per item):
///   1. Page is already loaded by <see cref="TestBase.BaseSetUp"/>.
///   2. Click the nav link.
///   3. Assert the target panel has the <c>active</c> CSS class.
///   4. Assert the URL hash has been updated by <c>history.replaceState</c>.
///   5. Assert the panel's heading is non-empty (language-agnostic).
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.None)]   // Tests share one driver instance per fixture; no intra-fixture parallelism.
public sealed class NavigationMenuTests : TestBase
{
    private HomePage _homePage = null!;

    // ── TestCaseSource ───────────────────────────────────────────────────────
    // Static: evaluated before any driver is started, so it only uses
    // the pure-data BuildNavItem factory (no IWebDriver dependency).

    private static IEnumerable<TestCaseData> NavItemTestCases()
        => HomePage.SectionIds.Select(id =>
            new TestCaseData(HomePage.BuildNavItem(id))
                .SetName($"Nav_{id}"));   // e.g."Nav_electrical", "Nav_contact"

    // ── Fixture SetUp ─────────────────────────────────────────────────────────

    [SetUp]
    public void SetUpPage()
    {
        // Driver is initialised by TestBase.BaseSetUp which runs first.
        _homePage = new HomePage(Driver, DefaultTimeout);
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Clicks each nav item and asserts the corresponding content panel becomes active.
    /// </summary>
    [Test, TestCaseSource(nameof(NavItemTestCases))]
    public void WhenNavItemClicked_CorrectPanelBecomesActive(NavItem item)
    {
        _homePage.ClickNavItem(item);
        _homePage.AssertPanelIsActive(item);
    }
}
