using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniValidation;
using RhenusTestApp;
using RhenusTestApp.Utils;
using static Microsoft.AspNetCore.Http.Results;

namespace RhenusTestApp.Services;


public static class Auth
{
    internal static async Task<IResult> RefreshTokenAsync
    (
        HttpRequest request,
        HttpResponse response,
        TokenValidator validator,
        TokenGenerator tokens,
        AppDbContext db
    )
    {
        var refreshToken = request.Cookies["refresh_token"];

        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest("Please include a refresh token in the request.");

        var tokenIsValid = validator.TryValidate(refreshToken, out var tokenId);
        if (!tokenIsValid) return BadRequest("Invalid refresh token.");

        var token = await db.Tokens.Where(token => token.Id == tokenId).FirstOrDefaultAsync();
        if (token is null) return BadRequest("Refresh token not found.");

        var user = await db.Users.Where(u => u.Id == token.UserId).FirstOrDefaultAsync();
        if (user is null) return BadRequest("User not found.");

        var accessToken = tokens.GenerateAccessToken(user);
        var (newRefreshTokenId, newRefreshToken) = tokens.GenerateRefreshToken();

        db.Tokens.Remove(token);
        await db.Tokens.AddAsync(new Token { Id = newRefreshTokenId, UserId = user.Id });
        await db.SaveChangesAsync();

        response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
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

    internal static async Task<IResult> SignOutAsync
    (
        HttpRequest request,
        HttpResponse response,
        AppDbContext db,
        TokenValidator validator
    )
    {
        var refreshToken = request.Cookies["refresh_token"];

        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest("Please include a refresh token in the request.");

        var tokenIsValid = validator.TryValidate(refreshToken, out var tokenId);
        if (!tokenIsValid) return BadRequest("Invalid refresh token.");

        var token = await db.Tokens.Where(token => token.Id == tokenId).FirstOrDefaultAsync();
        if (token is null) return BadRequest("Refresh token not found.");

        db.Tokens.Remove(token);
        await db.SaveChangesAsync();

        response.Cookies.Delete("refresh_token");
        return NoContent();
    }
}



