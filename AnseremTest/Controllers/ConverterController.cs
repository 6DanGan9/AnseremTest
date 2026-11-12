using AnseremTest.DataBase;
using AnseremTest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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

        [HttpPost("Сonvert")]
        public async Task<ActionResult<Email>> Convert(
            [FromBody] Email email)
        {
            var to = ParseAddresses(email.To);
            var copy = ParseAddresses(email.Copy);

            foreach (var address in to.Concat(copy))
            {
                if (!address.IsValid())
                {
                    _logger.LogWarning("Невалидный адрес: {Address}", address.Name);
                    return BadRequest(new { error = $"Невалидный адрес: {address.Name}" });
                }
            }

            var domains = await _context.Domains
                .AsNoTracking()
                .Include(d => d.StopList)
                .Include(d => d.SwitchList)
                .ToListAsync();

            var resultCopy = GetResultCopy(to, copy, domains);

            email.Copy = Format(resultCopy);
            return Ok(email);
        }

        private static List<Address> GetResultCopy(List<Address> to, List<Address> copy, List<Domain> domains)
        {
            List<Address> resultCopy = copy.ToList();

            foreach (var domain in domains)
            {
                var recipients = to.Concat(resultCopy).ToList();

                bool domainHit = recipients.Any(r =>
                    string.Equals(r.GetDomain(), domain.Name, StringComparison.OrdinalIgnoreCase));

                if (!domainHit)
                    continue;

                bool stopHit = recipients.Any(r =>
                    domain.StopList.Any(s =>
                        string.Equals(s.Name, r.Name, StringComparison.OrdinalIgnoreCase)));

                if (stopHit)
                {
                    resultCopy = resultCopy.Where(c => !domain.SwitchList.Any(s =>
                        string.Equals(s.Name, c.Name, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }
                else
                {
                    foreach (var sa in domain.SwitchList)
                    {
                        bool alreadyExists = resultCopy.Any(c =>
                            string.Equals(c.Name, sa.Name, StringComparison.OrdinalIgnoreCase));

                        if (!alreadyExists)
                            resultCopy.Add(new Address(sa.Name));
                    }
                }
            }

            return resultCopy;
        }

        private static List<Address> ParseAddresses(string? raw) =>
            string.IsNullOrWhiteSpace(raw)
                ? new List<Address>()
                : raw.Split("; ")
                     .Select(a => new Address(a))
                     .ToList();

        private static string Format(List<Address> addresses) =>
            addresses.Count switch
            {
                0 => string.Empty,
                1 => addresses[0].Name,
                _ => string.Join("; ", addresses.Select(a => a.Name))
            };

    }
}
