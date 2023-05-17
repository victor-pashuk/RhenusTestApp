
using System;
using Microsoft.AspNetCore.Identity;
using MiniValidation;
using RhenusTestApp;
using RhenusTestApp.Models;
using RhenusTestApp.Models.DTOs;
using RhenusTestApp.Utils;
using static Microsoft.AspNetCore.Http.Results;

namespace RhenusTestApp.Services;

public static class Users
{
    internal static async Task<IResult> SignUpAsync
    (
        UserManager<User> users,
        UserCreateDTO user,
        AppDbContext db
    )
    {
        if (user is null) return BadRequest();
        if (!MiniValidator.TryValidate(user, out var errors)) return BadRequest(errors);

        if (users.Users.Any(u => u.UserName == user.Username))
            return Conflict("Invalid `username`: A user with this username already exists.");

        var newUser = new User(user);
        var result = await users.CreateAsync(newUser, user.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);
        await db.GameProfiles.AddAsync(new GameProfile(newUser));
        await db.SaveChangesAsync();
        return Created($"/users/{newUser.Id}", newUser);
    }

    internal static async Task<IResult> SignInAsync
    (
        UserManager<User> users,
        TokenGenerator tokens,
        AppDbContext db,
        UserLoginDTO credentials,
        HttpResponse response
    )
    {
        if (credentials is null) return BadRequest();
        if (!MiniValidator.TryValidate(credentials, out var errors)) return BadRequest(errors);

        var user = await users.FindByNameAsync(credentials.Login);
        if (user is null) return NotFound("User with this username not found.");

        var result = await users.CheckPasswordAsync(user, credentials.Password);
        if (!result) return BadRequest("Incorrect password.");

        var accessToken = tokens.GenerateAccessToken(user);
        var (refreshTokenId, refreshToken) = tokens.GenerateRefreshToken();

        await db.Tokens.AddAsync(new Token { Id = refreshTokenId, UserId = user.Id });
        await db.SaveChangesAsync();

        response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            Expires = DateTime.Now.AddDays(1),
            HttpOnly = true,
            IsEssential = true,
            MaxAge = new TimeSpan(1, 0, 0, 0),
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        return Ok(accessToken);
    }
}

