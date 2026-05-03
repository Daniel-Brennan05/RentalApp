using RentalApp.Database.Models;
using System.Globalization;

namespace RentalApp.Converters;

/// <summary>
/// Returns true when a rental status is "Requested" (i.e. awaiting owner action).
/// Used to show the Approve/Reject buttons on the rentals page.
/// </summary>
public class StatusIsRequestedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() == RentalStatus.Requested;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
