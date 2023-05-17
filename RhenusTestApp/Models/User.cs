
using Microsoft.AspNetCore.Identity;
using RhenusTestApp.Models.DTOs;

namespace RhenusTestApp.Models;

public class User : IdentityUser<Guid>
{
    public string FullName { get; set; }
    public string Role { get; set; }
    public Guid? GameProfileId { get; set; }
    public virtual GameProfile? GameProfile { get; set; }

    public User() { }

    public User(UserCreateDTO dto)
    {
        FullName = dto.FullName;
        UserName = dto.Username;
        Role = "user";
    }
}
