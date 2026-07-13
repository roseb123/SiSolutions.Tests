#nullable enable

using SiSolutions.UiTests.Playwright.Pages;

namespace SiSolutions.UiTests.Playwright.Tests;

[TestFixture]
public sealed class NavigationScenarioDefinitionTests
{
    [Test]
    public void SectionIds_MatchExpectedNavigationScenarios()
    {
        var expected = new[]
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

        Assert.That(HomePage.SectionIds, Is.EqualTo(expected));
    }

    [Test]
    public void BuildNavItem_CreatesStableSelectors()
    {
        var item = HomePage.BuildNavItem("contact");

        Assert.Multiple(() =>
        {
            Assert.That(item.SectionId, Is.EqualTo("contact"));
            Assert.That(item.LinkSelector, Is.EqualTo("a.menu-link[data-section='contact']"));
            Assert.That(item.PanelSelector, Is.EqualTo("section.content-panel#contact"));
            Assert.That(item.HeadingSelector, Is.EqualTo("section#contact h2 span[data-i18n]"));
        });
    }
}
