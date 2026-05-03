using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalApp.Database.Models;

[Table("items")]
[PrimaryKey(nameof(Id))]
public class Item
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal DailyRate { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int OwnerId { get; set; }

    public bool IsAvailable { get; set; } = true;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public Point? Location { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    [ForeignKey(nameof(OwnerId))]
    public User Owner { get; set; } = null!;

    public List<Rental> Rentals { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
}
