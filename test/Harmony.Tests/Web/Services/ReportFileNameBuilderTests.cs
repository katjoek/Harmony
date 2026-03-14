namespace Harmony.Tests.Web.Services;

using Harmony.Web.Services;
using Xunit;

public sealed class ReportFileNameBuilderTests
{
    private static readonly DateTimeOffset FixedTime = new(2026, 3, 15, 14, 30, 5, TimeSpan.Zero);
    private const string FixedTimestamp = "20260315_143005";

    // -- Group reports --

    [Fact]
    public void Build_GroupReport_ContainsGroupName()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.Build("Group", "Bijbelkring Noord", null, 3);

        // Assert
        Assert.Contains("Bijbelkring_Noord", result);
    }

    [Fact]
    public void Build_GroupReport_ContainsTimestamp()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.Build("Group", "Bijbelkring Noord", null, 3);

        // Assert
        Assert.Contains(FixedTimestamp, result);
    }

    [Fact]
    public void Build_GroupReport_HasExpectedFormat()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.Build("Group", "Bijbelkring Noord", null, 3);

        // Assert
        Assert.Equal($"Bijbelkring_Noord_{FixedTimestamp}", result);
    }

    [Fact]
    public void Build_GroupReport_ReplacesSpacesWithUnderscores()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.Build("Group", "My Group Name", null, 3);

        // Assert
        Assert.DoesNotContain(" ", result);
        Assert.Contains("My_Group_Name", result);
    }

    // -- Birthday reports without group --

    [Fact]
    public void Build_BirthdayReportWithoutGroup_HasExpectedFormat()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.Build("Birthday", null, null, 3);

        // Assert
        Assert.Equal($"Verjaardagen_Maart_{FixedTimestamp}", result);
    }

    [Fact]
    public void Build_BirthdayReportWithoutGroup_HasExpectedFormatWithMei()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.Build("Birthday", null, null, 5);

        // Assert — format: Verjaardagen_{month}_{timestamp}, no group segment
        Assert.Equal($"Verjaardagen_Mei_{FixedTimestamp}", result);
    }

    // -- Birthday reports with group --

    [Fact]
    public void Build_BirthdayReportWithGroup_ContainsGroupName()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.Build("Birthday", null, "Bijbelkring Noord", 3);

        // Assert
        Assert.Contains("Bijbelkring_Noord", result);
    }

    [Fact]
    public void Build_BirthdayReportWithGroup_HasExpectedFormat()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.Build("Birthday", null, "Bijbelkring Noord", 3);

        // Assert
        Assert.Equal($"Verjaardagen_Bijbelkring_Noord_Maart_{FixedTimestamp}", result);
    }

    [Fact]
    public void Build_BirthdayReportWithGroup_ReplacesSpacesWithUnderscores()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.Build("Birthday", null, "My Group Name", 1);

        // Assert
        Assert.DoesNotContain(" ", result);
        Assert.Contains("My_Group_Name", result);
    }

    // -- Dutch month names --

    [Theory]
    [InlineData(1,  "Januari")]
    [InlineData(3,  "Maart")]
    [InlineData(6,  "Juni")]
    [InlineData(12, "December")]
    public void Build_BirthdayReport_ContainsDutchMonthName(int month, string expectedMonthName)
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.Build("Birthday", null, null, month);

        // Assert
        Assert.Contains(expectedMonthName, result);
    }

    // -- Helpers --

    private static ReportFileNameBuilder CreateBuilder() => new(new FixedTimeProvider(FixedTime));

    private sealed class FixedTimeProvider(DateTimeOffset fixedUtcTime) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => fixedUtcTime;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
