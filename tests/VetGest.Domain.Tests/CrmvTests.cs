using VetGest.Domain.ValueObjects;

namespace VetGest.Domain.Tests;

public sealed class CrmvTests
{
    [Theory]
    [InlineData("12345", "sp", "CRMV-SP 12345")]
    [InlineData(" 00123-V ", "RJ", "CRMV-RJ 123")]
    [InlineData("1", "df", "CRMV-DF 1")]
    public void TryCreate_normalizes_valid_registrations(string number, string state, string expected)
    {
        Assert.True(Crmv.TryCreate(number, state, out var crmv, out var error));
        Assert.Null(error);
        Assert.Equal(expected, crmv!.ToString());
    }

    [Theory]
    [InlineData("12345", "XX")]
    [InlineData("12345", "")]
    [InlineData("12345", null)]
    [InlineData("", "SP")]
    [InlineData("abc", "SP")]
    [InlineData("0000", "SP")]
    [InlineData("1234567", "SP")]
    public void TryCreate_rejects_invalid_registrations(string? number, string? state)
    {
        Assert.False(Crmv.TryCreate(number, state, out var crmv, out var error));
        Assert.Null(crmv);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }
}
