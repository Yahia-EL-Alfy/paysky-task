using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using paysky_task.Data;
using paysky_task.DTOs;
using paysky_task.Entities;
using System.Security.Claims;

namespace paysky_task.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Applicant")]
    public class ApplicationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ApplicationController> _logger;

        public ApplicationController(ApplicationDbContext context, IMemoryCache cache, ILogger<ApplicationController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchVacancies(string? query)
        {
            var cacheKey = $"vacancy_search_{query}";
            if (!_cache.TryGetValue(cacheKey, out List<VacancyViewDto> vacancyDtos))
            {
                var now = DateTime.UtcNow;
                var vacancies = await _context.Vacancies
                    .Where(v => v.IsActive && !v.IsArchived && v.ExpiryDate > now &&
                        (string.IsNullOrEmpty(query) || v.Title.Contains(query) || v.Description.Contains(query)))
                    .ToListAsync();
                vacancyDtos = vacancies.Select(v => new VacancyViewDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    Description = v.Description,
                    EmployerId = v.EmployerId,
                    MaxApplications = v.MaxApplications,
                    ExpiryDate = v.ExpiryDate,
                    IsActive = v.IsActive,
                    IsArchived = v.IsArchived
                }).ToList();
                _cache.Set(cacheKey, vacancyDtos, TimeSpan.FromMinutes(5));
            }
            return Ok(vacancyDtos);
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply(ApplicationDto dto)
        {
            var applicantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var vacancy = await _context.Vacancies.Include(v => v.Applications).FirstOrDefaultAsync(v => v.Id == dto.VacancyId);
            if (vacancy == null || !vacancy.IsActive || vacancy.IsArchived || vacancy.ExpiryDate <= DateTime.UtcNow)
                return BadRequest("Vacancy is not available.");
            if (vacancy.Applications.Count >= vacancy.MaxApplications)
                return BadRequest("Maximum number of applications reached.");
            var lastApplication = await _context.Applications
                .Where(a => a.ApplicantId == applicantId)
                .OrderByDescending(a => a.AppliedAt)
                .FirstOrDefaultAsync();
            if (lastApplication != null && (DateTime.UtcNow - lastApplication.AppliedAt).TotalHours < 24)
                return BadRequest("You can only apply for one vacancy per 24 hours.");
            var alreadyApplied = await _context.Applications.AnyAsync(a => a.ApplicantId == applicantId && a.VacancyId == dto.VacancyId);
            if (alreadyApplied)
                return BadRequest("You have already applied for this vacancy.");
            var application = new Application
            {
                ApplicantId = applicantId,
                VacancyId = dto.VacancyId,
                AppliedAt = DateTime.UtcNow
            };
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Applicant {applicantId} applied for vacancy {vacancy.Id}");
            return Ok(new { application.Id, application.VacancyId, application.ApplicantId, application.AppliedAt });
        }
    }
}
