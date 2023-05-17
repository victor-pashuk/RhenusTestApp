using System;
using RhenusTestApp.Models.DTOs;

namespace RhenusTestApp.Utils;

public static class GameResultGenerator
{

    public static GameResult GetResult(int number, double points, double account)
    {
        var isWin = new Random().Next(10) == number;
        if (isWin)
            return new GameResult { Points = $"+{points * 9}", Account = account + points * 9, Status = GameStatus.Win.ToString() };
        return new GameResult { Points = $"-{points}", Account = account - points, Status = GameStatus.Loss.ToString() };

    }
}


