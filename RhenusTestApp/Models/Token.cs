using System.ComponentModel.DataAnnotations;

namespace RhenusTestApp.Services;

public record Token
{
    [Key]
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
}