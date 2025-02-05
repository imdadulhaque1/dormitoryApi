using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs.admin.basicSetup.paidItemDto;
using snowtexDormitoryApi.DTOs.admin.settings.newPerson;
using snowtexDormitoryApi.Models.admin.basicSetup;
using snowtexDormitoryApi.Models.admin.settings;

namespace snowtexDormitoryApi.Controllers.Admin.settings
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize]
    public class NewPersonController : ControllerBase
    {
        private readonly AuthDbContext _context;
        public NewPersonController(AuthDbContext context)
        {
            _context = context;
        }
        private async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.userId == userId);
        }

        // POST api/newPerson
        [HttpPost]
        public async Task<IActionResult> CreateNewPerson([FromBody] postNewPersonDto postRequest)
        {
            if (postRequest == null || string.IsNullOrEmpty(postRequest.name) || string.IsNullOrEmpty(postRequest.companyName) || string.IsNullOrEmpty(postRequest.personalPhoneNo) || string.IsNullOrEmpty(postRequest.companyPhoneNo) || string.IsNullOrEmpty(postRequest.email) || string.IsNullOrEmpty(postRequest.nidBirthPassport) || string.IsNullOrEmpty(postRequest.countryName) )
            {
                return BadRequest(new { status = 400, message = "Invalid informations to add new person." });
            }

            if (!await UserExistsAsync(postRequest.createdBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            // Check if the person is already exists by contactNo or email
            var existingNewPerson= await _context.newPersonModels.FirstOrDefaultAsync(r => r.personalPhoneNo == postRequest.personalPhoneNo || r.email == postRequest.email);
            if (existingNewPerson != null)
            {
                return Conflict(new { status = 409, message = "Personal contact no or email already exists." });
            }

            var newPersonInfo = new NewPersonModel
            {
                name = postRequest.name,
                companyName = postRequest.companyName,
                personalPhoneNo = postRequest.personalPhoneNo,
                companyPhoneNo = postRequest.companyPhoneNo,
                email = postRequest.email,
                nidBirthPassport = postRequest.nidBirthPassport,
                countryName = postRequest.countryName,
                address = postRequest.address,
                createdBy = postRequest.createdBy,
                createdTime = DateTime.UtcNow,
                isApprove = false,
                isActive = true
            };

            _context.newPersonModels.Add(newPersonInfo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateNewPerson), new
            {
                status = 201,
                message = "New person created successfully!",
                Name = newPersonInfo.name
            });
        }

        // GET api/newPerson
        [HttpGet]
        public async Task<IActionResult> GetAllPersons([FromQuery] string search = "")
        {
            IQueryable<NewPersonModel> query = _context.newPersonModels
                .Where(p => p.isActive == true);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.name.Contains(search) ||
                    p.personalPhoneNo.Contains(search) ||
                    p.email.Contains(search)
                );
            }

            var personInfo = await query.ToListAsync();
            return Ok(new { status = 200, message = "Persons' info retrieved successfully.", data = personInfo });
        }
        //[HttpGet]
        //public async Task<IActionResult> GetAllPersons()
        //{
        //    var personInfo = await _context.newPersonModels
        //        .Where(f => f.isActive == true)
        //        .ToListAsync();
        //    return Ok(new { status = 200, message = "Person's info retrieved successfully.", data = personInfo });
        //}


        // PUT api/newPerson/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePersonInfo(int id, [FromBody] putNewPersonDto putRequest)
        {
            if (putRequest == null || string.IsNullOrEmpty(putRequest.name) || string.IsNullOrEmpty(putRequest.companyName) || string.IsNullOrEmpty(putRequest.personalPhoneNo) || string.IsNullOrEmpty(putRequest.companyPhoneNo) || string.IsNullOrEmpty(putRequest.email) || string.IsNullOrEmpty(putRequest.nidBirthPassport) || string.IsNullOrEmpty(putRequest.countryName))
            {
                return BadRequest(new { status = 400, message = "Invalid informations to update person's info" });
            }
            if (!await UserExistsAsync(putRequest.updatedBy))
            {
                return StatusCode(404, new { status = 404, message = "User not found" });
            }

            var modifiedPersonInfo= await _context.newPersonModels.FirstOrDefaultAsync(r => r.personId == id && r.isActive == true);
            if (modifiedPersonInfo == null)
            {
                return NotFound(new { status = 404, message = "Person not found on inactive." });
            }

            modifiedPersonInfo.name = putRequest.name;
            modifiedPersonInfo.companyName = putRequest.companyName;
            modifiedPersonInfo.personalPhoneNo = putRequest.personalPhoneNo;
            modifiedPersonInfo.companyPhoneNo = putRequest.companyPhoneNo;
            modifiedPersonInfo.email = putRequest.email;
            modifiedPersonInfo.nidBirthPassport = putRequest.nidBirthPassport;
            modifiedPersonInfo.countryName = putRequest.countryName;
            modifiedPersonInfo.address = putRequest.address;

            modifiedPersonInfo.updatedBy = putRequest.updatedBy;
            modifiedPersonInfo.updatedTime = DateTime.UtcNow;

            _context.newPersonModels.Update(modifiedPersonInfo);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Person's info updated successfully.", data = modifiedPersonInfo });
        }


        // DELETE api/newPerson/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteNewPerson(int id, [FromBody] deleteNewPersonDto deleteRequest)
        {
            // Validate if user exists
            if (!await UserExistsAsync(deleteRequest.inactiveBy))
            {
                return NotFound(new { status = 404, message = "User not found" });
            }

            // Find the Persons by id
            var deletedPerson= await _context.newPersonModels.FindAsync(id);

            if (deletedPerson == null || deletedPerson.isActive == false)
            {
                return NotFound(new { status = 404, message = "Person not found or already inactive" });
            }

            // Perform soft delete
            deletedPerson.isActive = false;
            deletedPerson.inactiveBy = deleteRequest.inactiveBy;
            deletedPerson.inactiveTime = DateTime.UtcNow;

            _context.newPersonModels.Update(deletedPerson);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "Person deleted successfully (soft delete)",
                data = deletedPerson
            });
        }



    }
}
