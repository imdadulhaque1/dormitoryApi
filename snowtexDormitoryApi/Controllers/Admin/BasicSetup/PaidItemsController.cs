using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs.admin.basicSetup.buildingDto;
using snowtexDormitoryApi.DTOs.admin.basicSetup.paidItemDto;
using snowtexDormitoryApi.Models.admin.basicSetup;

namespace snowtexDormitoryApi.Controllers.Admin.BasicSetup
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize] 
    public class PaidItemsController : ControllerBase
    {
        private readonly AuthDbContext _context;
        public PaidItemsController(AuthDbContext context)
        {
            _context = context;
        }
        private async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.userId == userId);
        }

        // POST api/paidItems
        [HttpPost]
        public async Task<IActionResult> CreatePaidItems([FromBody] postPaidItemDto postRequest)
        {
            if (postRequest == null || string.IsNullOrEmpty(postRequest.name) || string.IsNullOrEmpty(postRequest.price) || string.IsNullOrEmpty(postRequest.remarks))
            {
                return BadRequest(new { status = 400, message = "Invalid paid items data." });
            }

            if (!await UserExistsAsync(postRequest.createdBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            // Check if paid Items already exists by name
            var existingPaidItem= await _context.paidItemModels.FirstOrDefaultAsync(r => r.name == postRequest.name);
            if (existingPaidItem != null)
            {
                return Conflict(new { status = 409, message = "Paid items already exists." });
            }

            var newPaidItem = new PaidItemsModels
            {
                name = postRequest.name,
                price = postRequest.price,
                priceCalculateBy = postRequest.priceCalculateBy,
                remarks = postRequest.remarks,
                createdBy = postRequest.createdBy,
                createdTime = DateTime.UtcNow,
                isApprove = false,
                isActive = true
            };

            _context.paidItemModels.Add(newPaidItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreatePaidItems), new
            {
                status = 201,
                message = "Paid item created successfully!",
                Name = newPaidItem.name
            });
        }

        // GET api/paidItems
        [HttpGet]
        public async Task<IActionResult> GetPaidItems()
        {
            var paidItems = await _context.paidItemModels
                .Where(f => f.isActive == true)
                .ToListAsync();
            return Ok(new { status = 200, message = "Paid Items retrieved successfully.", data = paidItems });
        }


        // PUT api/paidItems/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePaidItems(int id, [FromBody] putPaidItemDto putRequest)
        {
            if (putRequest == null || string.IsNullOrEmpty(putRequest.name) || string.IsNullOrEmpty(putRequest.price) || string.IsNullOrEmpty(putRequest.remarks))
            {
                return BadRequest(new { status = 400, message = "Invalid paid items data." });
            }
            if (!await UserExistsAsync(putRequest.updatedBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            var modifiedPaidItem= await _context.paidItemModels.FirstOrDefaultAsync(r => r.paidItemId == id && r.isActive == true);
            if (modifiedPaidItem == null)
            {
                return NotFound(new { status = 404, message = "Paid item not found on inactive." });
            }

            modifiedPaidItem.name = putRequest.name;
            modifiedPaidItem.price = putRequest.price;
            modifiedPaidItem.priceCalculateBy = putRequest.priceCalculateBy;
            modifiedPaidItem.remarks = putRequest.remarks;

            modifiedPaidItem.updatedBy = putRequest.updatedBy;
            modifiedPaidItem.updatedTime = DateTime.UtcNow;

            _context.paidItemModels.Update(modifiedPaidItem);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Paid items updated successfully.", data = modifiedPaidItem });
        }


        // DELETE api/paidItens/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePaidItems(int id, [FromBody] deletePaidItemDto deleteRequest)
        {
            // Validate if user exists
            if (!await UserExistsAsync(deleteRequest.inactiveBy))
            {
                return NotFound(new { status = 404, message = "User not found" });
            }

            // Find the Paid Items
            var deletedPaidItem= await _context.paidItemModels.FindAsync(id);

            if (deletedPaidItem == null || deletedPaidItem.isActive == false)
            {
                return NotFound(new { status = 404, message = "Paid Item not found or already inactive" });
            }

            // Perform soft delete
            deletedPaidItem.isActive = false;
            deletedPaidItem.inactiveBy = deleteRequest.inactiveBy;
            deletedPaidItem.inactiveTime = DateTime.UtcNow;

            _context.paidItemModels.Update(deletedPaidItem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Paid Item deleted successfully (soft delete)",
                data = deletedPaidItem
            });
        }

    }
}
