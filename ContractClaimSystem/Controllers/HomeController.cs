using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ContractClaimSystem.Models;

namespace ContractClaimSystem.Controllers;
public class HomeController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    //The user Identity is related to our custom class that extends Identity
    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.FullName = user?.FullName ?? "User"; // Default to "User" if FullName is null
        }

        return View();
    }
}
