using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using paysky_task.Data;
using paysky_task.DTOs;
using paysky_task.Entities;
using System.Security.Claims;

namespace paysky_task.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employer")]
    public class VacancyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VacancyController> _logger;

        public VacancyController(ApplicationDbContext context, ILogger<VacancyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create(VacancyDto dto)
        {
            var employerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var vacancy = new Vacancy
            {
                Title = dto.Title,
                Description = dto.Description,
                EmployerId = employerId,
                MaxApplications = dto.MaxApplications,
                ExpiryDate = dto.ExpiryDate,
                IsActive = true,
                IsArchived = false
            };
            _context.Vacancies.Add(vacancy);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Vacancy created: {vacancy.Title} by Employer {employerId}");
            return Ok(vacancy);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, VacancyDto dto)
        {
            var vacancy = await _context.Vacancies.FindAsync(id);
            if (vacancy == null || vacancy.EmployerId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
                return NotFound();
            vacancy.Title = dto.Title;
            vacancy.Description = dto.Description;
            vacancy.MaxApplications = dto.MaxApplications;
            vacancy.ExpiryDate = dto.ExpiryDate;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Vacancy updated: {vacancy.Title} by Employer {vacancy.EmployerId}");
            return Ok(vacancy);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vacancy = await _context.Vacancies.FindAsync(id);
            if (vacancy == null || vacancy.EmployerId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
                return NotFound();
            _context.Vacancies.Remove(vacancy);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Vacancy deleted: {vacancy.Title} by Employer {vacancy.EmployerId}");
            return Ok();
        }

        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var vacancy = await _context.Vacancies.FindAsync(id);
            if (vacancy == null || vacancy.EmployerId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
                return NotFound();
            vacancy.IsActive = false;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Vacancy deactivated: {vacancy.Title} by Employer {vacancy.EmployerId}");
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var employerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var vacancies = await _context.Vacancies
                .Where(v => v.EmployerId == employerId)
                .ToListAsync();
            var vacancyDtos = vacancies.Select(v => new VacancyViewDto
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
            return Ok(vacancyDtos);
        }

        [HttpGet("{id}/applicants")]
        public async Task<IActionResult> GetApplicants(int id)
        {
            var vacancy = await _context.Vacancies
                .Include(v => v.Applications)
                .ThenInclude(a => a.Applicant)
                .FirstOrDefaultAsync(v => v.Id == id && v.EmployerId == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (vacancy == null)
                return NotFound();
            var applicants = vacancy.Applications.Select(a => new { a.ApplicantId, a.Applicant.Username, a.AppliedAt });
            return Ok(applicants);
        }
    }
}
