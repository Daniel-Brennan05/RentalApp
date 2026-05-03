using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalApp.Database.Models;

[Table("reviews")]
[PrimaryKey(nameof(Id))]
public class Review
{
    public int Id { get; set; }

    [Required]
    public int RentalId { get; set; }

    [Required]
    public int ReviewerId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string Comment { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(RentalId))]
    public Rental Rental { get; set; } = null!;

    [ForeignKey(nameof(ReviewerId))]
    public User Reviewer { get; set; } = null!;
}
