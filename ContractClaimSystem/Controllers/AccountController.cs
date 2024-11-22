using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ContractClaimSystem.Models;

namespace ContractClaimSystem.Controllers;
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    // GET: Register Page
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: Handle Registration
    [HttpPost]
    public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword, string role)
    {
        // Check if passwords match
        if (password != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
            return View();
        }

        var user = new ApplicationUser
        {
            FullName = fullName,
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            // Ensure the role exists, including the new "HR" role
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            // Assign the selected role to the user
            await _userManager.AddToRoleAsync(user, role);

            // Automatically sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        // Log any errors and return to the view
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View();
    }

    // GET: Login Page
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // POST: Handle Login
    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "User not found.");
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName, password, false, false);

        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }
        else if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account is locked out.");
        }
        else if (!await _userManager.CheckPasswordAsync(user, password))
        {
            ModelState.AddModelError(string.Empty, "Password is incorrect.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View();
    }

    // Log out
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
}
