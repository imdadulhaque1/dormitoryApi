using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs;
using snowtexDormitoryApi.DTOs.admin.tags;
using snowtexDormitoryApi.Models;

namespace snowtexDormitoryApi.Controllers.Admin.BasicSetup
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protect the entire controller, requiring a valid token
    public class TagController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public TagController(AuthDbContext context)
        {
            _context = context;
        }

        // POST api/role
        [HttpPost]
        public async Task<IActionResult> CreateTag([FromBody] TagPostRequestDto tagRequest)
        {
            if (tagRequest == null || string.IsNullOrEmpty(tagRequest.name) || string.IsNullOrEmpty(tagRequest.createdBy))
            {
                return BadRequest(new { status = 400, message = "Invalid tag data." });
            }

            // Check if role already exists by name
            var existingTag = await _context.Tags.FirstOrDefaultAsync(r => r.name == tagRequest.name);
            if (existingTag != null)
            {
                return Conflict(new { status = 409, message = "Tag already exists." });
            }

            var newTag= new TagModel
            {
                name = tagRequest.name,
                createdBy = tagRequest.createdBy,
                createdTime = DateTime.UtcNow,
                isApprove = false,
                isActive = true
            };

            _context.Tags.Add(newTag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateTag), new
            {
                status = 201,
                message = "Tag created successfully!",
                Name = newTag.name
            });
        }

        // GET api/tag
        [HttpGet]
        public async Task<IActionResult> GetTags()
        {
            var retrievedTags = await _context.Tags.ToListAsync();
            return Ok(new { status = 200, message = "Tags retrieved successfully.", retrievedTags });
        }

        // PUT api/tag/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTag(int id, [FromBody] TagPutRequestDto tagRequest)
        {
            if (tagRequest == null || string.IsNullOrEmpty(tagRequest.name))
            {
                return BadRequest(new { status = 400, message = "Invalid tag data." });
            }

            var tag = await _context.Tags.FirstOrDefaultAsync(r => r.tagId == id);
            if (tag == null)
            {
                return NotFound(new { status = 404, message = "Tag not found." });
            }

            tag.name = tagRequest.name;
            tag.updatedBy = tagRequest.updatedBy;
            tag.updatedTime = DateTime.UtcNow;

            _context.Tags.Update(tag);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Tag updated successfully.", tag });
        }

        // DELETE api/tag/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(r => r.tagId == id);
            if (tag == null)
            {
                return NotFound(new { status = 404, message = "Tag not found." });
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Tag deleted successfully." });
        }
    }
}
