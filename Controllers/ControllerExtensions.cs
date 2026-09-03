using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Api.Controllers
{
    // Small helper so every controller can grab "who is logged in right now"
    // from the JWT claims without repeating the same parsing code everywhere.
    public static class ControllerExtensions
    {
        public static int GetUserId(this ControllerBase controller)
        {
            var idClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(idClaim!);
        }
    }
}
