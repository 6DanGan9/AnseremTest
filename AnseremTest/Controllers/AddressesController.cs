using AnseremTest.DataBase;
using AnseremTest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnseremTest.Controllers
{
    public record CreateAddressDto(string Name);

    [ApiController]
    [Route("api/[controller]")]
    public class AddressesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AddressesController> _logger;

        public AddressesController(
            AppDbContext context,
            ILogger<AddressesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetAll(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            if (take <= 0 || take > 200)
                take = 50;

            var addresses = await _context.Addresses
                .AsNoTracking()
                .OrderBy(d => d.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return Ok(addresses);
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<Address>> GetByName(string name)
        {
            var address = await _context.Addresses.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Name == name);

            if (address == null)
            {
                _logger.LogWarning("Адрес {Name} не найден", name);
                return NotFound($"Адрес '{name}' не найден");
            }

            return address;
        }

        [HttpPost]
        public async Task<ActionResult<Address>> Create([FromBody] CreateAddressDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Адрес обязателен.");

            var address = new Address(dto.Name);

            try
            {
                if (!address.IsValid())
                    return BadRequest($"Некорректный формат адреса: '{dto.Name}'.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка валидации адреса {Name}", dto.Name);
                return BadRequest(ex.Message);
            }

            if (await _context.Addresses.AnyAsync(d => d.Name == dto.Name))
                return Conflict($"Аддресс '{dto.Name}' уже существует.");

            _context.Addresses.Add(address); 
            await _context.SaveChangesAsync();

            _logger.LogInformation("Создан адрес {Name}", address.Name);
            return CreatedAtAction(nameof(GetByName), new { name = address.Name }, address);
        }

        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Name == name);

            if (address == null)
                return NotFound($"Адрес '{name}' не найден.");

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Удалён адрес {Name}", name);
            return NoContent();
        }
    }
}
