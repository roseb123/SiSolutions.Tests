#nullable enable

using Microsoft.Playwright;

namespace SiSolutions.UiTests.Playwright.Infrastructure;

public static class Waits
{
    public static async Task WaitForVisibleAsync(this ILocator locator, TimeSpan timeout)
    {
        await locator.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = (float)timeout.TotalMilliseconds,
        });
    }

    public static async Task WaitForSelectorHasCssClassAsync(this IPage page, string selector, string cssClass, TimeSpan timeout)
    {
        await page.WaitForFunctionAsync(
            "([selector, cssClass]) => { const element = document.querySelector(selector); return !!element && element.classList.contains(cssClass); }",
            new object[] { selector, cssClass },
            new PageWaitForFunctionOptions
            {
                Timeout = (float)timeout.TotalMilliseconds,
            });
    }

    public static async Task WaitForUrlHashAsync(this IPage page, string expectedHash, TimeSpan timeout)
    {
        await page.WaitForFunctionAsync(
            "expectedHash => window.location.hash.toLowerCase() === `#${expectedHash}`.toLowerCase()",
            expectedHash,
            new PageWaitForFunctionOptions
            {
                Timeout = (float)timeout.TotalMilliseconds,
            });
    }

    public static async Task WaitForPageReadyAsync(this IPage page, TimeSpan timeout)
    {
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await page.WaitForFunctionAsync(
            "() => document.title && document.title.trim().length > 0",
            null,
            new PageWaitForFunctionOptions
            {
                Timeout = (float)timeout.TotalMilliseconds,
            });
    }
}