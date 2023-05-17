
using System.Security.Claims;
using RhenusTestApp.Models;
namespace RhenusTestApp.Utils;

public static class UserValidator
{
    public static bool TryValidate(ClaimsPrincipal claims, out User user, out string errorMessage)
    {
        if (!Guid.TryParse(claims.FindFirstValue("jti"), out var _))
            errorMessage = "Invalid `tokenId`: Not a valid GUID.";

        Guid UserId = Guid.Empty;
        string Username = string.Empty; ;
        user = null;

        errorMessage = string.Empty; ;
        var test = claims.Claims.ToList();
        var userIds = claims.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).ToArray();
        if (userIds.Length == 2) // only userid and username
        {
            if (Guid.TryParse(userIds[0].Value, out Guid userId0))
            {
                UserId = userId0;
                Username = userIds[1].Value;
            }
            else if (Guid.TryParse(userIds[1].Value, out Guid userId1))
            {
                UserId = userId1;
                Username = userIds[0].Value;
            }
            else
            {
                errorMessage = "Invalid userid";
                return false;
            }
        }
        else if (userIds.Length == 1) // only userid or only username
        {
            if (Guid.TryParse(userIds[0].Value, out Guid userId)) UserId = userId;
            else Username = userIds[0].Value;
        }
        else
        {
            errorMessage = "Invalid claims of type 'nameidentifier'";
            return false;
        }

        string FullName = claims.FindFirstValue(ClaimTypes.Name);
        string Role = claims.FindFirstValue(ClaimTypes.Role);

        user = new User
        {
            Id = UserId,
            UserName = Username,
            FullName = FullName,
            Role = Role
        };

        return true;
    }

    public static bool TryValidate(HttpRequest request, Guid userId, out string errorMessage)
    {
        errorMessage = string.Empty;
        UserValidator.TryValidate(request.HttpContext.User, out var user, out var errMsg);
        if (user.Id == userId)
            return true;
        errorMessage = errMsg;
        return false;

    }
}
