using RentalApp.Database.Data.Repositories;
using RentalApp.Database.Models;

namespace RentalApp.Services;

public class RentalService : IRentalService
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IItemRepository _itemRepository;

    public RentalService(IRentalRepository rentalRepository, IItemRepository itemRepository)
    {
        _rentalRepository = rentalRepository;
        _itemRepository = itemRepository;
    }

    public async Task<bool> CanRentItemAsync(int itemId, DateTime startDate, DateTime endDate)
    {
        startDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        endDate   = DateTime.SpecifyKind(endDate.Date,   DateTimeKind.Utc);

        if (startDate >= endDate)
            return false;

        var existingRentals = await _rentalRepository.GetByItemAsync(itemId);

        var hasConflict = existingRentals.Any(r =>
            (r.Status == RentalStatus.Approved || r.Status == RentalStatus.OutForRent) &&
            r.StartDate < endDate &&
            r.EndDate > startDate);

        return !hasConflict;
    }

    public async Task<Rental> RequestRentalAsync(int itemId, int borrowerId, DateTime startDate, DateTime endDate)
    {
        // Postgres timestamp with time zone requires UTC; DatePicker returns Local kind
        startDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        endDate   = DateTime.SpecifyKind(endDate.Date,   DateTimeKind.Utc);

        var item = await _itemRepository.GetByIdAsync(itemId)
            ?? throw new InvalidOperationException("Item not found.");

        if (!item.IsAvailable)
            throw new InvalidOperationException("This item is not available for rent.");

        if (!await CanRentItemAsync(itemId, startDate, endDate))
            throw new InvalidOperationException("This item is already booked for the selected dates.");

        var days = Math.Max(1, (endDate - startDate).Days);
        var rental = new Rental
        {
            ItemId = itemId,
            BorrowerId = borrowerId,
            StartDate = startDate,
            EndDate = endDate,
            Status = RentalStatus.Requested,
            TotalPrice = item.DailyRate * days
        };

        return await _rentalRepository.CreateAsync(rental);
    }

    public async Task ApproveRentalAsync(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidOperationException("Rental not found.");

        if (rental.Status != RentalStatus.Requested)
            throw new InvalidOperationException($"Cannot approve a rental with status '{rental.Status}'.");

        await _rentalRepository.UpdateStatusAsync(rentalId, RentalStatus.Approved);
    }

    public async Task RejectRentalAsync(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidOperationException("Rental not found.");

        if (rental.Status != RentalStatus.Requested)
            throw new InvalidOperationException($"Cannot reject a rental with status '{rental.Status}'.");

        await _rentalRepository.UpdateStatusAsync(rentalId, RentalStatus.Rejected);
    }

    public async Task StartRentalAsync(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidOperationException("Rental not found.");

        if (rental.Status != RentalStatus.Approved)
            throw new InvalidOperationException($"Cannot start a rental with status '{rental.Status}'.");

        await _rentalRepository.UpdateStatusAsync(rentalId, RentalStatus.OutForRent);
    }

    public async Task ReturnRentalAsync(int rentalId)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new InvalidOperationException("Rental not found.");

        if (rental.Status != RentalStatus.Approved && rental.Status != RentalStatus.OutForRent)
            throw new InvalidOperationException($"Cannot return a rental with status '{rental.Status}'.");

        await _rentalRepository.UpdateStatusAsync(rentalId, RentalStatus.Returned);
    }
}
