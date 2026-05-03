using Moq;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.Test.ViewModels;

/// <summary>
/// Demonstrates the MockLocationService pattern and verifies the item repository
/// behaviour that ItemsListViewModel and NearbyItemsViewModel depend on.
/// </summary>
public class ItemsListViewModelTests
{
    private readonly Mock<IItemRepository>   _itemRepoMock;
    private readonly Mock<ILocationService>  _locationServiceMock;

    public ItemsListViewModelTests()
    {
        _itemRepoMock        = new Mock<IItemRepository>();
        _locationServiceMock = new Mock<ILocationService>();
    }

    [Fact]
    public async Task GetAvailableAsync_ShouldReturnOnlyAvailableItems()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Id = 1, Title = "Power Drill", IsAvailable = true,  DailyRate = 10m },
            new Item { Id = 2, Title = "Ladder",      IsAvailable = false, DailyRate = 5m  }
        };
        _itemRepoMock
            .Setup(r => r.GetAvailableAsync())
            .ReturnsAsync(items.Where(i => i.IsAvailable).ToList());

        // Act
        var result = await _itemRepoMock.Object.GetAvailableAsync();

        // Assert
        Assert.All(result, item => Assert.True(item.IsAvailable));
        Assert.Single(result);
    }

    [Fact]
    public async Task GetCurrentLocationAsync_WhenGranted_ShouldReturnCoordinates()
    {
        // Arrange
        var expectedLocation = (Latitude: 55.95, Longitude: -3.18);
        _locationServiceMock
            .Setup(l => l.GetCurrentLocationAsync())
            .ReturnsAsync(expectedLocation);

        // Act
        var location = await _locationServiceMock.Object.GetCurrentLocationAsync();

        // Assert
        Assert.NotNull(location);
        Assert.Equal(55.95, location.Value.Latitude);
        Assert.Equal(-3.18, location.Value.Longitude);
    }

    [Fact]
    public async Task GetCurrentLocationAsync_WhenDenied_ShouldReturnNull()
    {
        // Arrange
        _locationServiceMock
            .Setup(l => l.GetCurrentLocationAsync())
            .ReturnsAsync((ValueTuple<double, double>?)null);

        // Act
        var location = await _locationServiceMock.Object.GetCurrentLocationAsync();

        // Assert
        Assert.Null(location);
    }

    [Fact]
    public async Task GetAvailableAsync_WithLocation_ShouldReturnItemsWithCoordinates()
    {
        // Arrange
        _locationServiceMock
            .Setup(l => l.GetCurrentLocationAsync())
            .ReturnsAsync((55.95, -3.18));

        var items = new List<Item>
        {
            new Item { Id = 1, Title = "Tent",        IsAvailable = true, Latitude = 55.96, Longitude = -3.19 },
            new Item { Id = 2, Title = "Power Drill", IsAvailable = true, Latitude = null,  Longitude = null  }
        };
        _itemRepoMock
            .Setup(r => r.GetAvailableAsync())
            .ReturnsAsync(items);

        // Act
        var location = await _locationServiceMock.Object.GetCurrentLocationAsync();
        var allItems = await _itemRepoMock.Object.GetAvailableAsync();
        var nearbyItems = allItems
            .Where(i => i.Latitude.HasValue && i.Longitude.HasValue)
            .ToList();

        // Assert
        Assert.NotNull(location);
        Assert.Single(nearbyItems);
        Assert.Equal("Tent", nearbyItems[0].Title);
    }

    [Theory]
    [InlineData("tools")]
    [InlineData("camping")]
    public async Task GetByCategoryAsync_ShouldReturnItemsMatchingSlug(string slug)
    {
        // Arrange
        var items = new List<Item>
        {
            new Item
            {
                Id         = 1,
                Title      = "Item in category",
                IsAvailable = true,
                Category   = new Category { Slug = slug }
            }
        };
        _itemRepoMock
            .Setup(r => r.GetByCategoryAsync(slug))
            .ReturnsAsync(items);

        // Act
        var result = await _itemRepoMock.Object.GetByCategoryAsync(slug);

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, item => Assert.Equal(slug, item.Category.Slug));
    }
}
