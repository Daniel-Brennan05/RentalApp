using RentalApp.Database.Models;
using System.Globalization;

namespace RentalApp.Converters;

/// <summary>
/// Maps a rental status string to a background colour for the status badge.
/// </summary>
public class StatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            RentalStatus.Requested  => Color.FromArgb("#ffc107"),
            RentalStatus.Approved   => Color.FromArgb("#28a745"),
            RentalStatus.Rejected   => Color.FromArgb("#dc3545"),
            RentalStatus.OutForRent => Color.FromArgb("#17a2b8"),
            RentalStatus.Overdue    => Color.FromArgb("#fd7e14"),
            RentalStatus.Returned   => Color.FromArgb("#6c757d"),
            RentalStatus.Completed  => Color.FromArgb("#512BD4"),
            _                       => Color.FromArgb("#6c757d"),
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
