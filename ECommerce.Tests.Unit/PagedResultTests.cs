using ECommerce.Application.Helpers;
using FluentAssertions;

namespace ECommerce.Tests.Unit;

public class PagedResultTests
{
    [Fact]
    public void Create_RoundsTotalPagesUp()
    {
        var result = PagedResult<int>.Create(Enumerable.Range(1, 10), totalCount: 25, page: 1, pageSize: 10);

        result.TotalPages.Should().Be(3);
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.HasNext.Should().BeTrue();
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void Create_WithExactDivision_ReturnsExactPageCount()
    {
        var result = PagedResult<int>.Create(Enumerable.Range(1, 10), totalCount: 20, page: 1, pageSize: 10);

        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public void Create_WithNoItems_ReturnsZeroPages()
    {
        var result = PagedResult<int>.Create(Enumerable.Empty<int>(), totalCount: 0, page: 1, pageSize: 10);

        result.TotalPages.Should().Be(0);
        result.HasNext.Should().BeFalse();
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void Create_OnLastPage_HasNextIsFalse()
    {
        var result = PagedResult<int>.Create(Enumerable.Range(21, 5), totalCount: 25, page: 3, pageSize: 10);

        result.HasNext.Should().BeFalse();
        result.HasPrevious.Should().BeTrue();
    }
}
