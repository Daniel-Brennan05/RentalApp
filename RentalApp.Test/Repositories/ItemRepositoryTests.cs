using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Test.Fixtures;

namespace RentalApp.Test.Repositories;

public class ItemRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ItemRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllItems()
    {
        // Arrange
        var repository = new ItemRepository(_fixture.Context);

        // Act
        var items = await repository.GetAllAsync();

        // Assert
        Assert.NotEmpty(items);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingItem_ShouldReturnItem()
    {
        // Arrange
        var repository = new ItemRepository(_fixture.Context);

        // Act
        var item = await repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(item);
        Assert.Equal("Power Drill", item.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentItem_ShouldReturnNull()
    {
        // Arrange
        var repository = new ItemRepository(_fixture.Context);

        // Act
        var item = await repository.GetByIdAsync(999);

        // Assert
        Assert.Null(item);
    }

    [Theory]
    [InlineData("tools")]
    [InlineData("camping")]
    public async Task GetByCategoryAsync_ShouldReturnCorrectItems(string categorySlug)
    {
        // Arrange
        var repository = new ItemRepository(_fixture.Context);

        // Act
        var items = await repository.GetByCategoryAsync(categorySlug);

        // Assert
        Assert.All(items, item => Assert.Equal(categorySlug, item.Category.Slug));
    }

    [Fact]
    public async Task GetAvailableAsync_ShouldReturnOnlyAvailableItems()
    {
        // Arrange
        var repository = new ItemRepository(_fixture.Context);

        // Act
        var items = await repository.GetAvailableAsync();

        // Assert
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.True(item.IsAvailable));
    }

    [Fact]
    public async Task GetByOwnerAsync_ShouldReturnOwnerItems()
    {
        // Arrange
        var repository = new ItemRepository(_fixture.Context);

        // Act
        var items = await repository.GetByOwnerAsync(ownerId: 1);

        // Assert
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.Equal(1, item.OwnerId));
    }

    [Fact]
    public async Task CreateAsync_ValidItem_ShouldPersistItem()
    {
        // Arrange
        var repository = new ItemRepository(_fixture.Context);
        var newItem = new Item
        {
            Title       = "Ladder",
            Description = "6ft step ladder",
            DailyRate   = 8.00m,
            CategoryId  = 1,
            OwnerId     = 1,
            IsAvailable = true
        };

        // Act
        var created = await repository.CreateAsync(newItem);

        // Assert
        Assert.True(created.Id > 0);
        Assert.Equal("Ladder", created.Title);
    }
}
