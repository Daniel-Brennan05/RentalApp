using RentalApp.Database.Models;
using System.Globalization;

namespace RentalApp.Converters;

/// <summary>
/// Returns true when a rental is in an active state (Approved or OutForRent).
/// Used to show the "Mark as Returned" button on the rentals page.
/// </summary>
public class StatusIsActiveConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value?.ToString();
        return status is RentalStatus.Approved or RentalStatus.OutForRent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
