namespace RentalApp.Database.Models;

public static class RentalStatus
{
    public const string Requested = "Requested";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string OutForRent = "OutForRent";
    public const string Overdue = "Overdue";
    public const string Returned = "Returned";
    public const string Completed = "Completed";

    public static readonly string[] AllStatuses =
    {
        Requested, Approved, Rejected, OutForRent, Overdue, Returned, Completed
    };
}
