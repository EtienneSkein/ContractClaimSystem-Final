using ContractClaimSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ContractClaimSystem.Controllers;

[Authorize]
public class ClaimsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<ClaimsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Claims/SubmitClaim
    [HttpGet]
    [Authorize(Roles = "Lecturer")]
    public async Task<IActionResult> SubmitClaim()
    {
        if (User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.FullName = user?.FullName ?? "Lecturer";
        }

        return View();
    }

    // POST: Claims/SubmitClaim
    [HttpPost]
    [Authorize(Roles = "Lecturer")]
    public async Task<IActionResult> SubmitClaim([Bind("HoursWorked,HourlyRate,AdditionalNotes")] ClaimSubmission claim, IFormFile? SupportingDocument)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            _logger.LogError("User ID is null.");
            return Unauthorized();
        }

        claim.UserId = userId;

        var lecturer = await _userManager.FindByIdAsync(userId);
        if (lecturer == null)
        {
            _logger.LogError("Lecturer not found for UserId.");
            ModelState.AddModelError("", "Lecturer not found.");
            return View(claim);
        }

        claim.Lecturer = lecturer;

        if (SupportingDocument != null && SupportingDocument.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, SupportingDocument.FileName);
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await SupportingDocument.CopyToAsync(stream);
                }
                claim.SupportingDocument = $"/uploads/{SupportingDocument.FileName}";
                _logger.LogInformation($"File uploaded successfully: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file: {ex.Message}");
                ModelState.AddModelError("", "File upload failed. Please try again.");
            }
        }

        ModelState.Remove("UserId");
        ModelState.Remove("Lecturer");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Claim submitted successfully for user ID: {userId}");
                return RedirectToAction("ClaimSubmitted");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving claim to database: {ex.Message}");
                ModelState.AddModelError("", "Failed to save the claim. Please try again.");
            }
        }
        else
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning($"Validation error: {error.ErrorMessage}");
            }
        }

        return View(claim);
    }

    public IActionResult ClaimSubmitted()
    {
        return View();
    }

    // GET: Claims/ManageClaims
    [HttpGet]
    [Authorize(Roles = "AcademicManager")]
    public async Task<IActionResult> ManageClaims()
    {
        var pendingClaims = await _context.Claims
            .Include(c => c.Lecturer)
            .Where(c => c.Status == "Pending")
            .ToListAsync();
        return View(pendingClaims);
    }

    [HttpPost]
    [Authorize(Roles = "AcademicManager")]
    public async Task<IActionResult> ApproveClaim(int claimId)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null)
        {
            return NotFound();
        }

        claim.Status = "Approved";
        await _context.SaveChangesAsync();

        return RedirectToAction("ManageClaims");
    }

    [HttpPost]
    [Authorize(Roles = "AcademicManager")]
    public async Task<IActionResult> RejectClaim(int claimId)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null)
        {
            return NotFound();
        }

        claim.Status = "Rejected";
        await _context.SaveChangesAsync();

        return RedirectToAction("ManageClaims");
    }

    // GET: Claims/ApprovedClaims
    [HttpGet]
    [Authorize(Roles = "AcademicManager")]
    public async Task<IActionResult> ApprovedClaims()
    {
        var approvedClaims = await _context.Claims
            .Include(c => c.Lecturer)
            .Where(c => c.Status == "Approved")
            .ToListAsync();
        return View(approvedClaims);
    }

    // GET: Claims/RejectedClaims
    [HttpGet]
    [Authorize(Roles = "AcademicManager")]
    public async Task<IActionResult> RejectedClaims()
    {
        var rejectedClaims = await _context.Claims
            .Include(c => c.Lecturer)
            .Where(c => c.Status == "Rejected")
            .ToListAsync();
        return View(rejectedClaims);
    }

    // GET: Claims/UploadDocuments
    [HttpGet]
    [Authorize(Roles = "Lecturer")]
    public async Task<IActionResult> UploadDocument()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var pendingClaims = await _context.Claims
            .Where(c => c.UserId == userId && c.Status == "Pending")
            .ToListAsync();

        return View(pendingClaims);
    }

    // POST: Claims/UploadAdditionalFiles
    [HttpPost]
    [Authorize(Roles = "Lecturer")]
    public async Task<IActionResult> UploadAdditionalFiles(int claimId, List<IFormFile> additionalFiles)
    {
        var claim = await _context.Claims.FindAsync(claimId);
        if (claim == null)
        {
            return NotFound("Claim not found.");
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", $"Claim_{claimId}");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        foreach (var file in additionalFiles)
        {
            if (file.Length > 0)
            {
                var filePath = Path.Combine(uploadsFolder, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
        }

        TempData["SuccessMessage"] = "Files uploaded successfully!";
        return RedirectToAction("UploadDocument");
    }
}
