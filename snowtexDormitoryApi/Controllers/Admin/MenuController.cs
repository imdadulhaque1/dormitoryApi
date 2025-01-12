using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs;
using snowtexDormitoryApi.DTOs.admin.menu;
using snowtexDormitoryApi.Models;
using snowtexDormitoryApi.Models.admin.menu;
using System;

namespace snowtexDormitoryApi.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protect the entire controller, requiring a valid token
    public class MenuController : ControllerBase
    {
        private readonly AuthDbContext _context;
        public MenuController(AuthDbContext context)
        {
            _context = context;
        }

        // POST api/menu
        [HttpPost]
        public async Task<IActionResult> CreateMenu([FromBody] MenuPostRequestDto menuRequest)
        {
            if (menuRequest == null || string.IsNullOrEmpty(menuRequest.banglaName) || string.IsNullOrEmpty(menuRequest.englishName) || string.IsNullOrEmpty(menuRequest.parentLayerId) || string.IsNullOrEmpty(menuRequest.htmlIcon) || string.IsNullOrEmpty(menuRequest.createdBy))
            {
                return BadRequest(new { status = 400, message = "Invalid Menu data." });
            }

            // Check if role already exists by englishName
            var existingRole = await _context.Menus.FirstOrDefaultAsync(r => r.englishName == menuRequest.englishName);
            if (existingRole != null)
            {
                return Conflict(new { status = 409, message = "Menu already exists." });
            }

            var newMenu = new MenuModel
            {
                banglaName = menuRequest.banglaName,
                englishName = menuRequest.englishName,
                url = menuRequest.url,
                parentLayerId = menuRequest.parentLayerId,
                menuSerialNo = menuRequest.menuSerialNo,
                htmlIcon = menuRequest.htmlIcon,

                createdBy = menuRequest.createdBy,
                createdTime = DateTime.UtcNow,
                isApprove = false,
                isActive = true
            };

            _context.Menus.Add(newMenu);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateMenu), new
            {
                status = 201,
                message = "Menu created successfully!",
                menu = new
                {
                    banglaName= newMenu.banglaName,
                    englishName=newMenu.englishName,
                    url = newMenu.url,
                    parentLayerId = newMenu.parentLayerId,
                    menuSerialNo = newMenu.menuSerialNo,
                    htmlIcon =newMenu.htmlIcon,
                }
            
            });
        }

        // GET api/menu
        //[HttpGet]
        //public async Task<IActionResult> GetMenus()
        //{
        //    var menus = await _context.Menus.ToListAsync();
        //    return Ok(new { status = 200, message = "Menu retrieved successfully.", menus });
        //}

        // GET api/menu
        [HttpGet]
        public async Task<IActionResult> GetMenus()
        {
            var menus = await _context.Menus
                                      .OrderBy(menu => menu.menuSerialNo) // Order by menuSerialNo
                                      .ToListAsync();

            return Ok(new { status = 200, message = "Menus retrieved successfully.", menus });
        }


        // PUT api/role/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenu(int id, [FromBody] MenuPutRequestDto menuRequest)
        {

            if (menuRequest == null || string.IsNullOrEmpty(menuRequest.englishName) || string.IsNullOrEmpty(menuRequest.banglaName) || string.IsNullOrEmpty(menuRequest.parentLayerId) || string.IsNullOrEmpty(menuRequest.htmlIcon))
            {
                return BadRequest(new { status = 400, message = "Invalid Menu data." });
            }

            var menu = await _context.Menus.FirstOrDefaultAsync(r => r.menuId == id);
            if (menu == null)
            {
                return NotFound(new { status = 404, message = "Menu not found." });
            }

            menu.banglaName = menuRequest.banglaName;
            menu.englishName = menuRequest.englishName;
            menu.url = menuRequest.url;
            menu.parentLayerId = menuRequest.parentLayerId;
            menu.menuSerialNo = menuRequest.menuSerialNo;
            menu.htmlIcon = menuRequest.htmlIcon;
            menu.updatedBy = menuRequest.updatedBy;
            menu.updatedTime = DateTime.UtcNow;

            _context.Menus.Update(menu);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Menu updated successfully.", menu });
        }

        // DELETE api/menu/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            var menu = await _context.Menus.FirstOrDefaultAsync(r => r.menuId == id);
            if (menu == null)
            {
                return NotFound(new { status = 404, message = "Menu not found." });
            }

            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();

            return Ok(new { status = 200, message = "Menu deleted successfully." });
        }
    }
}
