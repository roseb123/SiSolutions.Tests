#nullable enable

using Microsoft.Playwright;
using SiSolutions.UiTests.Playwright.Infrastructure;

namespace SiSolutions.UiTests.Playwright.Pages;

public sealed class HomePage
{
    private readonly IPage _page;
    private readonly TimeSpan _timeout;

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

    public HomePage(IPage page, TimeSpan timeout)
    {
        _page = page;
        _timeout = timeout;
    }

    public IReadOnlyList<NavItem> GetNavItems()
        => SectionIds.Select(BuildNavItem).ToList().AsReadOnly();

    public async Task ClickNavItemAsync(NavItem item)
    {
        var link = _page.Locator(item.LinkSelector);
        await link.WaitForVisibleAsync(_timeout);

        try
        {
            await link.ClickAsync(new LocatorClickOptions
            {
                Timeout = (float) _timeout.TotalMilliseconds,
            });
        }
        catch (PlaywrightException)
        {
            await link.ClickAsync(new LocatorClickOptions
            {
                Timeout = (float) _timeout.TotalMilliseconds,
                Force = true,
            });
        }
    }

    public async Task AssertPanelIsActiveAsync(NavItem item)
    {
        var panel = _page.Locator(item.PanelSelector);
        await _page.WaitForSelectorHasCssClassAsync(item.PanelSelector, "active", _timeout);
        await _page.WaitForUrlHashAsync(item.SectionId, _timeout);

        var heading = _page.Locator(item.HeadingSelector);
        await heading.WaitForVisibleAsync(_timeout);

        var headingText = (await heading.InnerTextAsync()).Trim();
        Assert.That(headingText, Is.Not.Empty, $"Heading inside section '#{item.SectionId}' was empty. (URL: {_page.Url} | Title: {await _page.TitleAsync()})");
    }

    public static NavItem BuildNavItem(string sectionId) => new(
        SectionId: sectionId,
        LinkSelector: $"a.menu-link[data-section='{sectionId}']",
        PanelSelector: $"section.content-panel#{sectionId}",
        HeadingSelector: $"section#{sectionId} h2 span[data-i18n]");

    public async Task<string> GetContactEmailAddressAsync()
    {
        var emailLink = _page.Locator("section#contact a[href^='mailto:']");
        await emailLink.WaitForVisibleAsync(_timeout);
        return (await emailLink.InnerTextAsync()).Trim();
    }

    public async Task<string> GetContactPhoneNumberAsync()
    {
        var phoneLink = _page.Locator("section#contact a[href^='tel:']");
        await phoneLink.WaitForVisibleAsync(_timeout);
        return (await phoneLink.InnerTextAsync()).Trim();
    }
}