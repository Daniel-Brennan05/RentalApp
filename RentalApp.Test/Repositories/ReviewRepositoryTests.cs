using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;
using RentalApp.Test.Fixtures;

namespace RentalApp.Test.Repositories;

public class ReviewRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ReviewRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByItemAsync_ItemWithReview_ShouldReturnReviews()
    {
        // Arrange
        var repository = new ReviewRepository(_fixture.Context);

        // Act
        var reviews = await repository.GetByItemAsync(itemId: 1);

        // Assert
        Assert.NotEmpty(reviews);
        Assert.All(reviews, r => Assert.NotNull(r.Reviewer));
    }

    [Fact]
    public async Task GetByItemAsync_ItemWithNoReviews_ShouldReturnEmpty()
    {
        // Arrange
        var repository = new ReviewRepository(_fixture.Context);

        // Act
        var reviews = await repository.GetByItemAsync(itemId: 2);

        // Assert
        Assert.Empty(reviews);
    }

    [Fact]
    public async Task GetAverageRatingAsync_ItemWithReviews_ShouldReturnCorrectAverage()
    {
        // Arrange
        var repository = new ReviewRepository(_fixture.Context);

        // Act
        var average = await repository.GetAverageRatingAsync(itemId: 1);

        // Assert — seeded rating is 4; CreateAsync may add a rating of 5 for the same item
        Assert.InRange(average, 4.0, 5.0);
    }

    [Fact]
    public async Task GetAverageRatingAsync_ItemWithNoReviews_ShouldReturnZero()
    {
        // Arrange
        var repository = new ReviewRepository(_fixture.Context);

        // Act
        var average = await repository.GetAverageRatingAsync(itemId: 2);

        // Assert
        Assert.Equal(0, average);
    }

    [Fact]
    public async Task GetByRentalAsync_ExistingReview_ShouldReturnReview()
    {
        // Arrange
        var repository = new ReviewRepository(_fixture.Context);

        // Act
        var review = await repository.GetByRentalAsync(rentalId: 1);

        // Assert
        Assert.NotNull(review);
        Assert.Equal(4, review.Rating);
    }

    [Fact]
    public async Task GetByRentalAsync_NonExistentRental_ShouldReturnNull()
    {
        // Arrange
        var repository = new ReviewRepository(_fixture.Context);

        // Act
        var review = await repository.GetByRentalAsync(rentalId: 999);

        // Assert
        Assert.Null(review);
    }

    [Fact]
    public async Task CreateAsync_ValidReview_ShouldPersistReview()
    {
        // Arrange
        var repository = new ReviewRepository(_fixture.Context);

        // Need a rental to attach the review to — use a new one with a unique ID
        var rental = new Rental
        {
            Id         = 50,
            ItemId     = 1,
            BorrowerId = 2,
            StartDate  = DateTime.Today.AddDays(-3),
            EndDate    = DateTime.Today.AddDays(-1),
            Status     = RentalStatus.Returned,
            TotalPrice = 30.00m
        };
        _fixture.Context.Rentals.Add(rental);
        await _fixture.Context.SaveChangesAsync();

        var review = new Review
        {
            RentalId   = 50,
            ReviewerId = 2,
            Rating     = 5,
            Comment    = "Fantastic drill!"
        };

        // Act
        var created = await repository.CreateAsync(review);

        // Assert
        Assert.True(created.Id > 0);
        Assert.Equal(5, created.Rating);
    }
}
