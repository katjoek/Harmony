namespace Harmony.E2ETests.Infrastructure;

using Microsoft.Playwright;
using Xunit;

internal static class DualListPageExtensions
{
    internal static async Task SelectAllOptionsAsync(this IPage page, string selectId)
    {
        await page.EvaluateAsync(
            "id => { const el = document.getElementById(id); for (const o of el.options) o.selected = true; el.dispatchEvent(new Event('change', { bubbles: true })); }",
            selectId);
        await page.WaitForTimeoutAsync(200);
    }

    internal static async Task AssertNoOptionsSelectedAsync(this IPage page, string selectId)
    {
        var selectedCount = await page.EvaluateAsync<int>(
            "id => document.getElementById(id).querySelectorAll('option:checked').length",
            selectId);
        Assert.Equal(0, selectedCount);
    }

    internal static async Task<string[]> GetOptionTextsAsync(this IPage page, string selectId)
    {
        return await page.EvaluateAsync<string[]>(
            "id => Array.from(document.getElementById(id).options).map(o => o.text)",
            selectId);
    }

    internal static async Task<string> GetFirstOptionValueAsync(this IPage page, string selectId)
    {
        return await page.EvaluateAsync<string>(
            "id => document.getElementById(id).options[0]?.value ?? ''",
            selectId);
    }

    internal static async Task DoubleClickOptionAsync(this IPage page, string selectId, string optionValue)
    {
        await page.EvaluateAsync(
            "([id, value]) => { const o = document.querySelector(`#${id} option[value='${value}']`); o.dispatchEvent(new MouseEvent('dblclick', { bubbles: true })); }",
            new[] { selectId, optionValue });
    }
}
