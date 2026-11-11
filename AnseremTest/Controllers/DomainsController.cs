using AnseremTest.DataBase;
using AnseremTest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AnseremTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DomainController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DomainController> _logger;

        public DomainController(
            AppDbContext context,
            ILogger<DomainController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DomainDto>>> GetAll(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            if (take <= 0 || take > 200)
                take = 50;

            var domains = await _context.Domains
                .AsNoTracking()
                .OrderBy(d => d.Name)
                .Skip(skip)
                .Take(take)
                .Select(d => new DomainDto
                {
                    Name = d.Name,
                    StopListCount = d.StopList.Count,
                    SwitchListCount = d.SwitchList.Count
                })
                .ToListAsync();

            return Ok(domains);
        }

        [HttpGet("{name}")]
        public async Task<ActionResult<DomainDetailsDto>> GetByName(string name)
        {
            var domain = await _context.Domains
                .AsNoTracking()
                .Include(d => d.StopList)
                .Include(d => d.SwitchList)
                .FirstOrDefaultAsync(d => d.Name == name);

            if (domain == null)
            {
                _logger.LogWarning("Домен {Name} не найден", name);
                return NotFound($"Домен '{name}' не найден.");
            }

            return Ok(new DomainDetailsDto
            {
                Name = domain.Name,
                StopList = domain.StopList.Select(a => a.Name).ToList(),
                SwitchList = domain.SwitchList.Select(a => a.Name).ToList()
            });
        }

        [HttpPost]
        public async Task<ActionResult<DomainDto>> Create([FromBody] CreateDomainDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Имя домена обязательно.");

            Domain domain = new Domain(dto.Name);
            try
            {
                if (!domain.IsValid())
                    return BadRequest($"Некорректное имя домена: '{dto.Name}'.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка валидации домена {Name}", dto.Name);
                return BadRequest(ex.Message);
            }

            if (await _context.Domains.AnyAsync(d => d.Name == dto.Name))
                return Conflict($"Домен '{dto.Name}' уже существует.");

            _context.Domains.Add(domain);
            await _context.SaveChangesAsync();

            var result = new DomainDto
            {
                Name = domain.Name,
                StopListCount = 0,
                SwitchListCount = 0
            };

            _logger.LogInformation("Создан домен {Name}", result.Name);
            return CreatedAtAction(nameof(GetByName), new { name = domain.Name }, result);
        }

        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            var domain = await _context.Domains
                .FirstOrDefaultAsync(d => d.Name == name);

            if (domain == null)
                return NotFound($"Домен '{name}' не найден.");

            _context.Domains.Remove(domain);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Удалён домен {Name}", name);
            return NoContent();
        }

        [HttpPost("{name}/stop/{addressName}")]
        public async Task<IActionResult> AddToStopList(string name, string addressName)
            => await ModifyList(name, addressName, add: true, isStopList: true);

        [HttpDelete("{name}/stop/{addressName}")]
        public async Task<IActionResult> RemoveFromStopList(string name, string addressName)
            => await ModifyList(name, addressName, add: false, isStopList: true);

        [HttpPost("{name}/switch/{addressName}")]
        public async Task<IActionResult> AddToSwitchList(string name, string addressName)
            => await ModifyList(name, addressName, add: true, isStopList: false);

        [HttpDelete("{name}/switch/{addressName}")]
        public async Task<IActionResult> RemoveFromSwitchList(string name, string addressName)
            => await ModifyList(name, addressName, add: false, isStopList: false);

        private async Task<IActionResult> ModifyList(
            string domainName,
            string addressName,
            bool add,
            bool isStopList)
        {
            var domain = await _context.Domains
                .Include(d => d.StopList)
                .Include(d => d.SwitchList)
                .FirstOrDefaultAsync(d => d.Name == domainName);

            if (domain == null)
                return NotFound($"Домен '{domainName}' не найден.");

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Name == addressName);

            if (address == null)
                return NotFound($"Адрес '{addressName}' не найден.");

            var list = isStopList ? domain.StopList : domain.SwitchList;

            if (add)
            {
                if (list.Any(a => a.Name == addressName))
                    return Conflict($"Адрес '{addressName}' уже есть в списке.");

                list.Add(address);
            }
            else
            {
                var existing = list.FirstOrDefault(a => a.Name == addressName);
                if (existing == null)
                    return NotFound($"Адрес '{addressName}' отсутствует в списке.");

                list.Remove(existing);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class DomainDto
    {
        public string Name { get; set; } = string.Empty;
        public int StopListCount { get; set; }
        public int SwitchListCount { get; set; }
    }

    public class DomainDetailsDto
    {
        public string Name { get; set; } = string.Empty;
        public List<string> StopList { get; set; } = new();
        public List<string> SwitchList { get; set; } = new();
    }

    public class CreateDomainDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateDomainDto
    {
        public string NewName { get; set; } = string.Empty;
    }
}
