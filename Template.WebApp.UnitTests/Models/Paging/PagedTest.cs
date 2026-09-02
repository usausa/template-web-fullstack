namespace Template.WebApp.Models.Paging;

public sealed class PagedTest
{
    [Fact]
    public void FirstPageHasNoPrev()
    {
        // Arrange
        var paged = CreatePaged(1, 10, 10, 25);

        // Act & Assert
        Assert.False(paged.HasPrev);
        Assert.True(paged.HasNext);
    }

    [Fact]
    public void LastPageHasNoNext()
    {
        // Arrange
        var paged = CreatePaged(3, 10, 5, 25);

        // Act & Assert
        Assert.True(paged.HasPrev);
        Assert.False(paged.HasNext);
    }

    [Fact]
    public void TotalPageIsCeiling()
    {
        // Arrange
        var paged = CreatePaged(1, 10, 10, 25);

        // Act & Assert
        Assert.Equal(3, paged.TotalPage);
    }

    [Fact]
    public void EmptyResultTotalPageIsOne()
    {
        // Arrange
        var paged = CreatePaged(1, 10, 0, 0);

        // Act & Assert
        Assert.Equal(1, paged.TotalPage);
        Assert.False(paged.IsOver);
    }

    [Fact]
    public void OverPageIsDetected()
    {
        // Arrange
        var paged = CreatePaged(5, 10, 0, 25);

        // Act & Assert
        Assert.True(paged.IsOver);
    }

    private static Paged<int> CreatePaged(int page, int size, int itemCount, int total)
    {
        var pageable = new TestCondition { Page = page, Size = size };
        // ReSharper disable once UseCollectionExpression
        return new Paged<int>(pageable, Enumerable.Range(0, itemCount).ToList(), total);
    }

    private sealed class TestCondition : Pageable;
}
