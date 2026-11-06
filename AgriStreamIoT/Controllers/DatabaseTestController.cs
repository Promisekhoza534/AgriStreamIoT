using AgriStreamIoT.Data;
using Microsoft.AspNetCore.Mvc;

namespace AgriStreamIoT.Controllers
{
    public class DatabaseTestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DatabaseTestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                bool connected = await _context.Database.CanConnectAsync();

                if (connected)
                {
                    return Content("SUCCESS: Connected to AgriStreamDB!");
                }

                return Content("Connection failed. Check your SQL settings.");
            }
            catch (Exception ex)
            {
                return Content("Database error: " + ex.Message);
            }
        }
    }
}