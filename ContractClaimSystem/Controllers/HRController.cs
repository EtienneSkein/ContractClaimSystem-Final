using ContractClaimSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace ContractClaimSystem.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<HRController> _logger;

        public HRController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<HRController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: HR/LecturerList
        [HttpGet]
        public async Task<IActionResult> LecturerList()
        {
            var lecturers = await _userManager.GetUsersInRoleAsync("Lecturer");
            return View(lecturers);
        }

        // POST: HR/GenerateReport
        [HttpPost]
        public async Task<IActionResult> GenerateReport(int claimId)
        {
            // Configure QuestPDF license
            QuestPDF.Settings.License = LicenseType.Community;

            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.Id == claimId);

            if (claim == null)
            {
                return NotFound("Claim not found.");
            }

            // Ensure the directory exists
            var reportsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "reports");
            if (!Directory.Exists(reportsFolder))
            {
                Directory.CreateDirectory(reportsFolder);
            }

            var filePath = Path.Combine(reportsFolder, $"ClaimReport_{claimId}.pdf");

            try
            {
                // Generate the PDF using QuestPDF
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    GenerateClaimReport(claim).GeneratePdf(stream);
                }

                // Return the generated report as a downloadable file
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, "application/pdf", $"ClaimReport_{claimId}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating report: {ex.Message}");
                return BadRequest("An error occurred while generating the report.");
            }
        }

        // Helper method to define the report layout
        private static IDocument GenerateClaimReport(ClaimSubmission claim)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    // Header Section
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("ContractClaimSystem").FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                            column.Item().Text("Claim Report").FontSize(16).SemiBold().FontColor(Colors.Grey.Darken2);
                        });

                        row.ConstantItem(100).Height(60).Image("C:/Etienne/Old/ContractClaimSystem/ContractClaimSystem/logo.png", ImageScaling.FitArea); // Optional: Add your logo here
                    });

                    // Content Section
                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(15);

                        // Claim Information Section
                        column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
                        {
                            row.RelativeItem().Text("Claim Information").FontSize(16).Bold();
                        });

                        column.Item().Column(innerColumn =>
                        {
                            innerColumn.Spacing(8);
                            innerColumn.Item().Text($"Claim ID: {claim.Id}").Bold();
                            innerColumn.Item().Text($"Lecturer Name: {claim.Lecturer?.FullName ?? "Unknown"}");
                            innerColumn.Item().Text($"Date Submitted: {claim.ClaimDate:yyyy-MM-dd}");
                            innerColumn.Item().Text($"Status: {claim.Status}")
                                .FontColor(claim.Status == "Approved" ? Colors.Green.Medium : Colors.Red.Medium);
                        });

                        // Claim Details Section
                        column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
                        {
                            row.RelativeItem().Text("Claim Details").FontSize(16).Bold();
                        });

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Description").FontSize(12).Bold();
                                header.Cell().Element(CellStyle).Text("Value").FontSize(12).Bold();
                            });

                            table.Cell().Element(CellStyle).Text("Hours Worked");
                            table.Cell().Element(CellStyle).Text($"{claim.HoursWorked}");

                            table.Cell().Element(CellStyle).Text("Hourly Rate");
                            table.Cell().Element(CellStyle).Text($"{claim.HourlyRate:C}");

                            table.Cell().Element(CellStyle).Text("Total Payment").Bold();
                            table.Cell().Element(CellStyle).Text($"{claim.FinalPayment:C}").Bold();
                        });

                        // Additional Notes Section
                        if (!string.IsNullOrEmpty(claim.AdditionalNotes))
                        {
                            column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
                            {
                                row.RelativeItem().Text("Additional Notes").FontSize(16).Bold();
                            });
                        }
                    });

                    // Footer Section
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Generated by ContractClaimSystem").FontSize(10).Italic();
                        text.Span($" | Date: {DateTime.Now:yyyy-MM-dd HH:mm}");
                    });
                });
            });

            // Define a cell style for reuse
            static IContainer CellStyle(IContainer container)
            {
                return container
                    .Padding(5)
                    .BorderBottom(1)
                    .BorderColor(Colors.Grey.Lighten3);
            }
        }


        // GET: HR/ClaimsList
        [HttpGet]
        public async Task<IActionResult> ClaimsList()
        {
            var approvedClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == "Approved")
                .ToListAsync();

            return View(approvedClaims);
        }

        // GET: HR/EditLecturer/{id}
        [HttpGet]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> EditLecturer(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var lecturer = await _userManager.FindByIdAsync(id);
            if (lecturer == null)
                return NotFound();

            return View(lecturer);
        }

        // POST: HR/EditLecturer/{id}
        [HttpPost]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> EditLecturer(string id, ApplicationUser updatedLecturer)
        {
            if (id != updatedLecturer.Id)
                return BadRequest();

            var lecturer = await _userManager.FindByIdAsync(id);
            if (lecturer == null)
                return NotFound();

            lecturer.FullName = updatedLecturer.FullName;
            lecturer.Email = updatedLecturer.Email;
            lecturer.UserName = updatedLecturer.Email;

            var result = await _userManager.UpdateAsync(lecturer);
            if (result.Succeeded)
                return RedirectToAction(nameof(LecturerList));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(lecturer);
        }
    }
}
