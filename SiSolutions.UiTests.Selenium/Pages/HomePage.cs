#nullable enable

using OpenQA.Selenium;
using SiSolutions.UiTests.Infrastructure;

namespace SiSolutions.UiTests.Pages;

/// <summary>
/// Page Object for https://si-solutions-bg.vercel.app.
///
/// Site mechanics (verified from source):
///   - Navigation is a tab-panel system, NOT a scroll-to-anchor system.
///   - Clicking a <c>.menu-link</c> calls JS <c>setActiveSection()</c> which:
///       1. Toggles the <c>active</c> CSS class on <c>.content-panel</c> sections.
///       2. Calls <c>history.replaceState()</c> to update the URL hash.
///   - The active panel is visible (<c>display: block</c>); inactive panels are hidden (<c>display: none</c>).
///
/// Selectors are CSS-attribute-based (<c>data-section</c>, <c>data-panel</c>) for stability.
/// If the site markup changes, update <see cref="SectionIds"/> and <see cref="BuildNavItem"/>.
/// </summary>
public sealed class HomePage
{
    private readonly IWebDriver _driver;
    private readonly TimeSpan   _timeout;

    // ── Section registry (single source of truth) ───────────────────────────
    // Matches the <a class="menu-link" data-section="..."> values in nav.menu.
    // Update this list if sections are added, removed, or renamed in index.html.
    public static readonly IReadOnlyList<string> SectionIds = new[]
    {
        "electrical",
        "cctv-sot",
        "access-control",
        "structured-cabling",
        "ups",
        "ev-chargers",
        "finishing",
        "why-us",
        "contact",
    };

    public HomePage(IWebDriver driver, TimeSpan timeout)
    {
        _driver  = driver;
        _timeout = timeout;
    }

    // ── Nav-item enumeration ─────────────────────────────────────────────────

    /// <summary>
    /// Returns an immutable list of <see cref="NavItem"/> records derived from
    /// <see cref="SectionIds"/>. No live DOM query is required — locators are
    /// built deterministically from the section-id list.
    /// </summary>
    public IReadOnlyList<NavItem> GetNavItems()
        => SectionIds.Select(BuildNavItem).ToList().AsReadOnly();

    // ── Interaction ──────────────────────────────────────────────────────────

    /// <summary>
    /// Clicks the nav link for <paramref name="item"/>.
    /// Falls back to a JS click if the element is intercepted (e.g. by an overlay).
    /// </summary>
    public void ClickNavItem(NavItem item)
    {
        var link = _driver.WaitForElementVisible(item.LinkLocator, _timeout);

        try
        {
            link.Click();
        }
        catch (ElementClickInterceptedException)
        {
            // Occasional overlap with the sticky lead bar — JS click bypasses it.
            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("arguments[0].click();", link);
        }
    }

    // ── Assertions ───────────────────────────────────────────────────────────

    /// <summary>
    /// Asserts that navigation to <paramref name="item"/> succeeded by verifying:
    ///   1. The target panel has the <c>active</c> CSS class  → panel is visible.
    ///   2. The URL hash ends with <c>#{SectionId}</c>        → <c>history.replaceState</c> fired.
    ///   3. The heading inside the panel is non-empty          → language-agnostic content check.
    /// </summary>
    public void AssertPanelIsActive(NavItem item)
    {
        var contextInfo = $"(URL: {_driver.Url} | Title: {_driver.Title})";

        // 1 · Panel acquires the "active" class (display: block becomes visible).
        _driver.WaitForElementHasCssClass(item.PanelLocator, "active", _timeout);

        // 2 · URL hash reflects the navigated section.
        _driver.WaitForUrlHash(item.SectionId, _timeout);

        // 3 · Heading text in the active panel is non-empty (works for BG and EN).
        var heading     = _driver.WaitForElementVisible(item.HeadingLocator, _timeout);
        var headingText = heading.Text.Trim();

        Assert.That(
            headingText, Is.Not.Empty,
            $"Heading inside section '#{item.SectionId}' was empty. {contextInfo}");
    }

    // ── Static factory (also used by TestCaseSource so it needs no driver) ───

    /// <summary>
    /// Builds a <see cref="NavItem"/> record from a section id.
    /// Locators mirror the markup:
    ///   Link    → <c>a.menu-link[data-section='id']</c>
    ///   Panel   → <c>section.content-panel#id</c>
    ///   Heading → <c>section#id h2 span[data-i18n]</c>
    /// </summary>
    public static NavItem BuildNavItem(string sectionId) => new(
        SectionId:      sectionId,
        LinkLocator:    By.CssSelector($"a.menu-link[data-section='{sectionId}']"),
        PanelLocator:   By.CssSelector($"section.content-panel#{sectionId}"),
        HeadingLocator: By.CssSelector($"section#{sectionId} h2 span[data-i18n]"));

    // ── Contact section helpers ───────────────────────────────────────────────

    /// <summary>
    /// Returns the email address text displayed inside the contact panel.
    /// <para>
    /// Selector: <c>section#contact a[href^="mailto:"]</c> — targets the
    /// <c>&lt;a href="mailto:…"&gt;</c> element whose visible text is the raw address.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The contact panel must be <em>active</em> (CSS class <c>active</c>, <c>display: block</c>)
    /// before calling this — navigate to it via <see cref="ClickNavItem"/> first,
    /// otherwise <see cref="IWebElement.Text"/> returns an empty string on a hidden element.
    /// </remarks>
    /// <returns>The trimmed email string the user sees, e.g. <c>sisolutionsbg@gmail.com</c>.</returns>
    public string GetContactEmailAddress()
    {
        var emailLocator = By.CssSelector("section#contact a[href^='mailto:']");
        var emailLink    = _driver.WaitForElementVisible(emailLocator, _timeout);
        return emailLink.Text.Trim();
    }

    /// <summary>
    /// Returns the phone number text displayed inside the contact panel.
    /// Selector: <c>section#contact a[href^="tel:"]</c>.
    /// The contact panel must be active before calling this.
    /// </summary>
    /// <returns>The trimmed phone string the user sees, e.g. <c>0876703085</c>.</returns>
    public string GetContactPhoneNumber()
    {
        var phoneLocator = By.CssSelector("section#contact a[href^='tel:']");
        var phoneLink    = _driver.WaitForElementVisible(phoneLocator, _timeout);
        return phoneLink.Text.Trim();
    }
}
