using ContractClaimSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

//Do not change the routing settings!!!
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services with ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>() // Use ApplicationUser
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add authentication and authorization services
builder.Services.AddAuthentication(); // Adds authentication services
builder.Services.AddAuthorization();  // Adds authorization services

// Add MVC or controllers with views
builder.Services.AddControllersWithViews();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
    options.User.RequireUniqueEmail = true;
});

var app = builder.Build();

// Ensure roles are created at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CreateRoles(services); // Seed roles here
}

// Configure the HTTP request pipeline
app.UseStaticFiles();   

app.UseRouting();

app.UseAuthentication(); // This must come before UseAuthorization
app.UseAuthorization();  // Ensures authorization is handled

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

// CreateRoles method - Needs to be after app.Run()
async Task CreateRoles(IServiceProvider serviceProvider)
{
    // Get the role manager from the service provider
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Define the roles to create, including the new "HR" role
    string[] roles = { "Lecturer", "AcademicManager", "HR" };

    // Loop through each role and create it if it doesn't exist
    foreach (var role in roles)
    {
        var roleExist = await roleManager.RoleExistsAsync(role);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
