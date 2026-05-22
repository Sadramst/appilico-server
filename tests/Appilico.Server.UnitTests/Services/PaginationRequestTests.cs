using Appilico.Server.Business.DTOs.Common;
using FluentAssertions;

namespace Appilico.Server.UnitTests.Services;

public class PaginationRequestTests
{
    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(-5, -10, 1, 1)]
    [InlineData(2, 500, 2, 50)]
    [InlineData(3, 25, 3, 25)]
    public void Normalize_ClampsPageAndPageSize(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        var result = PaginationRequest.Normalize(page, pageSize);

        result.Page.Should().Be(expectedPage);
        result.PageSize.Should().Be(expectedPageSize);
    }
}