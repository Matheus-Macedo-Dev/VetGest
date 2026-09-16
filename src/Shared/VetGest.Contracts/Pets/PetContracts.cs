using System.ComponentModel.DataAnnotations;

namespace VetGest.Contracts.Pets;

public sealed class PetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public decimal? CurrentWeight { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreatePetRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Species { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Breed { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [Range(typeof(decimal), "0.01", "999.99")]
    public decimal? CurrentWeight { get; set; }

    [StringLength(500)]
    public string? PhotoUrl { get; set; }
}