#nullable enable

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SiSolutions.UiTests.Infrastructure;

/// <summary>
/// Extension methods on <see cref="IWebDriver"/> providing explicit-wait helpers.
/// All methods use <see cref="WebDriverWait"/> — no <c>Thread.Sleep</c> anywhere.
/// </summary>
public static class Waits
{
    // ── Core wait ──────────────────────────────────────────────────────────

    /// <summary>
    /// Polls <paramref name="condition"/> until it returns a non-null / non-default
    /// value, or throws <see cref="WebDriverTimeoutException"/> on expiry.
    /// </summary>
    public static TResult WaitFor<TResult>(
        this IWebDriver driver,
        Func<IWebDriver, TResult?> condition,
        TimeSpan timeout,
        string timeoutMessage = "Condition was not satisfied within the timeout period.")
    {
        var wait = new WebDriverWait(driver, timeout)
        {
            Message = timeoutMessage,
        };
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        return wait.Until(condition)!;
    }

    // ── Element visibility ─────────────────────────────────────────────────

    /// <summary>
    /// Waits until the element identified by <paramref name="locator"/> is present
    /// in the DOM and has <c>Displayed == true</c>.
    /// </summary>
    public static IWebElement WaitForElementVisible(
        this IWebDriver driver,
        By locator,
        TimeSpan timeout)
    {
        return driver.WaitFor(
            d =>
            {
                var el = d.FindElement(locator);
                return el.Displayed ? el : null;
            },
            timeout,
            $"Element '{locator}' was not visible after {timeout.TotalSeconds}s.");
    }

    // ── CSS class presence ─────────────────────────────────────────────────

    /// <summary>
    /// Waits until the element identified by <paramref name="locator"/> has
    /// the specified <paramref name="cssClass"/> in its class list.
    /// Handles stale-element exceptions transparently through the ignore list.
    /// </summary>
    public static IWebElement WaitForElementHasCssClass(
        this IWebDriver driver,
        By locator,
        string cssClass,
        TimeSpan timeout)
    {
        return driver.WaitFor(
            d =>
            {
                var el = d.FindElement(locator);
                // GetDomAttribute is the non-obsolete replacement for GetAttribute in Selenium 4.x+.
                var classes = (el.GetDomAttribute("class") ?? string.Empty)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                return classes.Contains(cssClass) ? el : null;
            },
            timeout,
            $"Element '{locator}' did not acquire CSS class '{cssClass}' after {timeout.TotalSeconds}s.");
    }

    // ── URL hash ───────────────────────────────────────────────────────────

    /// <summary>
    /// Waits until the current URL ends with <c>#{expectedHash}</c>.
    /// Uses <see cref="WebDriverWait"/> with a <c>bool</c> condition directly so
    /// that <c>false</c> (the default) correctly signals "retry" to the wait loop.
    /// </summary>
    public static void WaitForUrlHash(
        this IWebDriver driver,
        string expectedHash,
        TimeSpan timeout)
    {
        // NOTE: WebDriverWait retries while the condition returns default(TResult).
        // For bool, default = false, so the wait retries until the condition returns true.
        // Do NOT route this through WaitFor<TResult?> — bool? breaks the null-sentinel logic.
        var wait = new WebDriverWait(driver, timeout)
        {
            Message = $"URL hash did not become '#{expectedHash}' after {timeout.TotalSeconds}s. " +
                      $"Current URL: {driver.Url}",
        };
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
        wait.Until(d => d.Url.EndsWith($"#{expectedHash}", StringComparison.OrdinalIgnoreCase));
    }
}
