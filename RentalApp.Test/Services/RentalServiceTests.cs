using Moq;
using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Services;

namespace RentalApp.Test.Services;

public class RentalServiceTests
{
    private readonly Mock<IRentalRepository> _rentalRepoMock;
    private readonly Mock<IItemRepository>   _itemRepoMock;
    private readonly RentalService           _rentalService;

    public RentalServiceTests()
    {
        _rentalRepoMock = new Mock<IRentalRepository>();
        _itemRepoMock   = new Mock<IItemRepository>();
        _rentalService  = new RentalService(_rentalRepoMock.Object, _itemRepoMock.Object);
    }

    [Fact]
    public async Task CanRentItemAsync_NoConflict_ShouldReturnTrue()
    {
        // Arrange
        _rentalRepoMock
            .Setup(r => r.GetByItemAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Rental>());

        var startDate = DateTime.Today.AddDays(1);
        var endDate   = DateTime.Today.AddDays(3);

        // Act
        var result = await _rentalService.CanRentItemAsync(itemId: 1, startDate, endDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanRentItemAsync_OverlappingApprovedRental_ShouldReturnFalse()
    {
        // Arrange
        var existing = new List<Rental>
        {
            new Rental
            {
                ItemId    = 1,
                Status    = RentalStatus.Approved,
                StartDate = DateTime.Today.AddDays(2),
                EndDate   = DateTime.Today.AddDays(5)
            }
        };
        _rentalRepoMock
            .Setup(r => r.GetByItemAsync(1))
            .ReturnsAsync(existing);

        // Act — requested dates overlap with existing approved rental
        var result = await _rentalService.CanRentItemAsync(
            itemId: 1,
            startDate: DateTime.Today.AddDays(3),
            endDate:   DateTime.Today.AddDays(6));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CanRentItemAsync_EndDateBeforeStartDate_ShouldReturnFalse()
    {
        // Arrange
        _rentalRepoMock
            .Setup(r => r.GetByItemAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Rental>());

        // Act
        var result = await _rentalService.CanRentItemAsync(
            itemId: 1,
            startDate: DateTime.Today.AddDays(5),
            endDate:   DateTime.Today.AddDays(1));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RequestRentalAsync_ValidRequest_ShouldCalculateTotalPrice()
    {
        // Arrange
        var item = new Item { Id = 1, DailyRate = 10.00m, IsAvailable = true };
        _itemRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);
        _rentalRepoMock.Setup(r => r.GetByItemAsync(1)).ReturnsAsync(new List<Rental>());
        _rentalRepoMock.Setup(r => r.CreateAsync(It.IsAny<Rental>()))
            .ReturnsAsync((Rental r) => r);

        var startDate = DateTime.Today.AddDays(1);
        var endDate   = DateTime.Today.AddDays(4); // 3 days

        // Act
        var rental = await _rentalService.RequestRentalAsync(
            itemId: 1, borrowerId: 2, startDate, endDate);

        // Assert
        Assert.Equal(30.00m, rental.TotalPrice);  // £10 * 3 days
        Assert.Equal(RentalStatus.Requested, rental.Status);
    }

    [Fact]
    public async Task ApproveRentalAsync_RequestedRental_ShouldSetApprovedStatus()
    {
        // Arrange
        var rental = new Rental { Id = 1, Status = RentalStatus.Requested };
        _rentalRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _rentalRepoMock.Setup(r => r.UpdateStatusAsync(1, RentalStatus.Approved))
            .Returns(Task.CompletedTask);

        // Act
        await _rentalService.ApproveRentalAsync(rentalId: 1);

        // Assert
        _rentalRepoMock.Verify(r => r.UpdateStatusAsync(1, RentalStatus.Approved), Times.Once);
    }

    [Fact]
    public async Task RejectRentalAsync_RequestedRental_ShouldSetRejectedStatus()
    {
        // Arrange
        var rental = new Rental { Id = 1, Status = RentalStatus.Requested };
        _rentalRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _rentalRepoMock.Setup(r => r.UpdateStatusAsync(1, RentalStatus.Rejected))
            .Returns(Task.CompletedTask);

        // Act
        await _rentalService.RejectRentalAsync(rentalId: 1);

        // Assert
        _rentalRepoMock.Verify(r => r.UpdateStatusAsync(1, RentalStatus.Rejected), Times.Once);
    }

    [Theory]
    [InlineData(RentalStatus.Approved)]
    [InlineData(RentalStatus.OutForRent)]
    public async Task ReturnRentalAsync_ActiveRental_ShouldSetReturnedStatus(string initialStatus)
    {
        // Arrange
        var rental = new Rental { Id = 1, Status = initialStatus };
        _rentalRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _rentalRepoMock.Setup(r => r.UpdateStatusAsync(1, RentalStatus.Returned))
            .Returns(Task.CompletedTask);

        // Act
        await _rentalService.ReturnRentalAsync(rentalId: 1);

        // Assert
        _rentalRepoMock.Verify(r => r.UpdateStatusAsync(1, RentalStatus.Returned), Times.Once);
    }

    [Fact]
    public async Task ApproveRentalAsync_AlreadyApproved_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var rental = new Rental { Id = 1, Status = RentalStatus.Approved };
        _rentalRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _rentalService.ApproveRentalAsync(rentalId: 1));
    }

    [Fact]
    public async Task StartRentalAsync_ApprovedRental_ShouldSetOutForRentStatus()
    {
        // Arrange
        var rental = new Rental { Id = 1, Status = RentalStatus.Approved };
        _rentalRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);
        _rentalRepoMock.Setup(r => r.UpdateStatusAsync(1, RentalStatus.OutForRent))
            .Returns(Task.CompletedTask);

        // Act
        await _rentalService.StartRentalAsync(rentalId: 1);

        // Assert
        _rentalRepoMock.Verify(r => r.UpdateStatusAsync(1, RentalStatus.OutForRent), Times.Once);
    }

    [Fact]
    public async Task StartRentalAsync_NotApproved_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var rental = new Rental { Id = 1, Status = RentalStatus.Requested };
        _rentalRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rental);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _rentalService.StartRentalAsync(rentalId: 1));
    }
}
