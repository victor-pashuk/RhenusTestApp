using System;
using Microsoft.EntityFrameworkCore;
using MiniValidation;
using RhenusTestApp.Models.DTOs;
using RhenusTestApp.Utils;
using static Microsoft.AspNetCore.Http.Results;


namespace RhenusTestApp.Services;

public static class Game
{

    internal static async Task<IResult> PlayAsync(HttpRequest request,
            HttpResponse response, AppDbContext db, GamePlayDTO? gamePlayDTO)
    {
        if (gamePlayDTO == null)
            return BadRequest("Body is missed");
        var parseResult = Guid.TryParse(gamePlayDTO.UserId, out Guid userId);
        if (!parseResult)
            return BadRequest("Wrong userId");
        if (!UserValidator.TryValidate(request, userId, out var errMsg))
            return Results.Forbid();

        if (!MiniValidator.TryValidate(gamePlayDTO, out var errors)) return BadRequest(errors);

        var gameProfile = db.GameProfiles.Include(x => x.User).FirstOrDefault(x => x.User.Id == userId);
        if (gameProfile == null)
            return BadRequest("Game is not found");
        var gameResult = GameResultGenerator.GetResult(gamePlayDTO.Number, gamePlayDTO.Points, gameProfile.Account);
        gameProfile.Account = gameResult.Account;
        db.GameProfiles.Update(gameProfile);
        await db.SaveChangesAsync();
        return Ok(gameResult);

    }
}


