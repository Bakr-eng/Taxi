using Microsoft.Extensions.Logging;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;

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

        public async Task<List<Driver>> GetAllAsync()
            => await _repo.GetAllAsync();

        public async Task<Driver?> GetByIdAsync(int id)
            => await _repo.GetByIdAsync(id);

        public async Task<Driver?> GetByUserIdAsync(string userId)
            => await _repo.GetByUserIdAsync(userId);

        public async Task CreateAsync(string name, string phone, string license, string? userId = null)
        {
            var driver = new Driver
            {
                Name = name,
                PhoneNumber = phone,
                LicenseNumber = license,
                IsAvailable = true,
                UserId = userId
            };
            await _repo.AddAsync(driver);
            _logger.LogInformation("Driver created: {DriverName}", name);
        }

        public async Task UpdateAsync(Driver driver)
            => await _repo.UpdateAsync(driver);

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
            _logger.LogInformation("Driver {DriverId} deleted", id);
        }
    }
}
