using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZoneSync.Core.Data;
using ZoneSync.Core.Entities.Identity;

namespace ZoneSync.Web.Controllers
{
    // Handles the "active farm" the signed-in user is currently viewing.
    // The selected FarmId is stored in Session so every other module
    // (Dashboard, Zones, Crops, Sensors, Alerts, Tasks) can read
    // Session["ActiveFarmId"] and scope its queries to that farm.
    [Authorize]
    public class FarmContextController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FarmContextController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Called from the farm switcher dropdown in the sidebar/topbar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Switch(int farmId, string? returnUrl = null)
        {
            var aspNetUserId = _userManager.GetUserId(User);
            var profile = await _db.UserProfiles
                .FirstOrDefaultAsync(u => u.AspNetUserId == aspNetUserId);

            if (profile is null)
            {
                return Forbid();
            }

            var isMember = await _db.FarmMemberships
                .AnyAsync(m => m.UserId == profile.UserId && m.FarmId == farmId);

            if (!isMember)
            {
                // User isn't a member of that farm — ignore the switch attempt
                // rather than trusting an arbitrary farmId from the client.
                return Forbid();
            }

            HttpContext.Session.SetInt32(FarmContext.ActiveFarmSessionKey, farmId);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("FarmDashboard", "Dashboard");
        }
    }

    // Small static holder for the session key name so both the controller
    // and _Layout.cshtml reference the exact same string.
    public static class FarmContext
    {
        public const string ActiveFarmSessionKey = "ActiveFarmId";
    }
}
