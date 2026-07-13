#nullable enable

using OpenQA.Selenium;

namespace SiSolutions.UiTests.Pages;

/// <summary>
/// Immutable model for a single top navigation item and its related section.
/// Using a <c>record</c> gives structural equality and a clean <c>ToString()</c>
/// without boilerplate — section IDs are the natural display name in test output.
/// </summary>
/// <param name="SectionId">The value of the <c>data-section</c> attribute (e.g. "electrical").</param>
/// <param name="LinkLocator">Selenium locator targeting the nav anchor element.</param>
/// <param name="PanelLocator">Selenium locator targeting the content-panel section element.</param>
/// <param name="HeadingLocator">Selenium locator targeting the <c>&lt;h2&gt;</c> heading span inside the panel.</param>
public sealed record NavItem(
    string SectionId,
    By     LinkLocator,
    By     PanelLocator,
    By     HeadingLocator)
{
    /// <summary>Used as the NUnit test-case name in parameterised tests.</summary>
    public override string ToString() => SectionId;
}
