using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ContractClaimSystem.Models;

public class ApplicationDbContextTests
{
    [Fact]
    public async Task Can_Add_Claim_To_Database()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Test_Claim_DB")
            .Options;

        // Arrange
        using (var context = new ApplicationDbContext(options))
        {
            var claim = new ClaimSubmission
            {
                HoursWorked = 5,
                HourlyRate = 15,
                AdditionalNotes = "Test notes"
            };

            // Act
            context.Claims.Add(claim);
            await context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, await context.Claims.CountAsync());
        }
    }
}
