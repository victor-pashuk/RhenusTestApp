using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace RhenusTestApp.Models;

public class GameProfile
{
    [Key]
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public virtual User? User { get; set; }
    public double Account { get; set; }

    public GameProfile()
    {
        Account = 10000;
    }

    public GameProfile(User user)
    {
        UserId = user.Id;
        Account = 10000;
    }
}


