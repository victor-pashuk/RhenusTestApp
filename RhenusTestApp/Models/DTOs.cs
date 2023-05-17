using System.ComponentModel.DataAnnotations;
using RhenusTestApp.Models;

namespace RhenusTestApp.Models.DTOs;

public record UserDTO
{
    public string Id { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string FullName { get; set; }

    [Required]
    public string Role { get; set; }


    public User ToEntity()
    {
        return new User
        {
            Id = Guid.TryParse(Id, out Guid UserId) ? UserId : Guid.NewGuid(),
            UserName = UserName,
            FullName = FullName,
            Role = Role,
        };
    }
}

public record UserCreateDTO
{
    [Required]
    [DataType(DataType.Password), MinLength(8)]
    public string Password { get; set; }

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }

    [Required]
    public string FullName { get; set; }

    [MinLength(4)]
    [Required]
    public string Username { get; set; }

    [Required]
    public string Role { get; set; }

}

public record UserLoginDTO
{
    [Required]
    public string Login { get; set; }

    [Required]
    public string Password { get; set; }
}

public record GamePlayDTO
{
    [Required]
    public string UserId { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public double Points { get; set; }

    [Required]
    [Range(1, 9)]
    public int Number { get; set; }
}

public record GameResult
{
    public string Points { get; set; }
    public string Status { get; set; }
    public double Account { get; set; }
}

