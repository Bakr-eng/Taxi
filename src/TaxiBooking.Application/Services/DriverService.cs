using Microsoft.Extensions.Logging;
using TaxiBooking.Application.DTOs;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Application.Mappings;

namespace TaxiBooking.Application.Services
{
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _repo;
        private readonly ILogger<DriverService> _logger;

        public DriverService(IDriverRepository repo, ILogger<DriverService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<DriverDto>> GetAllAsync()
            => (await _repo.GetAllAsync())
                .OrderBy(d => d.Name)
                .Select(d => d.ToDto())
                .ToList();

        public async Task<List<DriverDto>> GetAvailableAsync()
            => (await _repo.GetAllAsync())
                .Where(d => d.IsAvailable)
                .OrderBy(d => d.Name)
                .Select(d => d.ToDto())
                .ToList();

        public async Task<DriverDto?> GetByIdAsync(int id)
            => (await _repo.GetByIdAsync(id))?.ToDto();

        public async Task<DriverDto?> GetByUserIdAsync(string userId)
            => (await _repo.GetByUserIdAsync(userId))?.ToDto();

        public async Task CreateAsync(CreateDriverDto dto)
        {
            await _repo.AddAsync(dto.ToEntity());
            _logger.LogInformation("Driver created: {DriverName}", dto.Name);
        }

        public async Task ToggleAvailabilityAsync(int id)
        {
            var driver = await _repo.GetByIdAsync(id);
            if (driver == null) return;
            driver.IsAvailable = !driver.IsAvailable;
            await _repo.UpdateAsync(driver);
        }

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
            _logger.LogInformation("Driver {DriverId} deleted", id);
        }
    }
}