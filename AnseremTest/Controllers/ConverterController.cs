using AnseremTest.DataBase;
using Microsoft.AspNetCore.Mvc;

namespace AnseremTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConverterController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ConverterController> _logger;

        public ConverterController(
            AppDbContext context,
            ILogger<ConverterController> logger)
        {
            _context = context;
            _logger = logger;
        }


    }
}
