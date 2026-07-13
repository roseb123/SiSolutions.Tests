#nullable enable

using Microsoft.Playwright;

namespace SiSolutions.UiTests.Playwright.Pages;

public sealed class HomePage
{
    private readonly IPage _page;
    private readonly float _timeoutMs;

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

    public HomePage(IPage page, float timeoutMs)
    {
        _page = page;
        _timeoutMs = timeoutMs;
    }

    public IReadOnlyList<NavItem> GetNavItems()
        => SectionIds.Select(BuildNavItem).ToList().AsReadOnly();

    public async Task ClickNavItem(NavItem item)
    {
        var link = _page.Locator(item.LinkSelector);
        await link.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = _timeoutMs });

        try
        {
            await link.ClickAsync(new LocatorClickOptions { Timeout = _timeoutMs });
        }
        catch (PlaywrightException)
        {
            await link.EvaluateAsync("el => el.click()");
        }
    }

    public async Task AssertPanelIsActive(NavItem item)
    {
        var panel = _page.Locator(item.PanelSelector);
        await panel.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = _timeoutMs });

        await _page.WaitForFunctionAsync(
            "([selector, cssClass]) => document.querySelector(selector)?.classList.contains(cssClass) === true",
            new object[] { item.PanelSelector, "active" },
            new PageWaitForFunctionOptions { Timeout = _timeoutMs });

        await _page.WaitForFunctionAsync(
            "expectedHash => window.location.hash.toLowerCase() === ('#' + expectedHash.toLowerCase())",
            item.SectionId,
            new PageWaitForFunctionOptions { Timeout = _timeoutMs });

        var heading = _page.Locator(item.HeadingSelector);
        await heading.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = _timeoutMs });

        var headingText = (await heading.InnerTextAsync()).Trim();

        Assert.That(
            headingText,
            Is.Not.Empty,
            $"Heading inside section '#{item.SectionId}' was empty. URL={_page.Url} | Title={await _page.TitleAsync()}");
    }

    public static NavItem BuildNavItem(string sectionId) => new(
        SectionId: sectionId,
        LinkSelector: $"a.menu-link[data-section='{sectionId}']",
        PanelSelector: $"section.content-panel#{sectionId}",
        HeadingSelector: $"section#{sectionId} h2 span[data-i18n]");

    public async Task<string> GetContactEmailAddress()
    {
        var emailLink = _page.Locator("section#contact a[href^='mailto:']");
        await emailLink.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = _timeoutMs });
        return (await emailLink.InnerTextAsync()).Trim();
    }

    public async Task<string> GetContactPhoneNumber()
    {
        var phoneLink = _page.Locator("section#contact a[href^='tel:']");
        await phoneLink.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = _timeoutMs });
        return (await phoneLink.InnerTextAsync()).Trim();
    }
}
