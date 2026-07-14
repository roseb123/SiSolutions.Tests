#nullable enable

namespace SiSolutions.UiTests.Playwright.Pages;

public sealed record NavItem(
    string SectionId,
    string LinkSelector,
    string PanelSelector,
    string HeadingSelector)
{
    public override string ToString() => SectionId;
}