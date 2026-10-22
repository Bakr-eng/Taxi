using Microsoft.Extensions.Logging;
using TaxiBooking.Application.DTOs;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Application.Mappings;

namespace TaxiBooking.Application.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _repo;
        private readonly ILogger<CarService> _logger;

        public CarService(ICarRepository repo, ILogger<CarService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<CarDto>> GetAllAsync()
            => (await _repo.GetAllAsync())
                .OrderBy(c => c.RegistrationNumber)
                .Select(c => c.ToDto())
                .ToList();

        public async Task CreateAsync(CreateCarDto dto)
        {
            await _repo.AddAsync(dto.ToEntity());
            _logger.LogInformation("Car {RegNr} created", dto.RegistrationNumber);
        }

        public async Task AssignDriverAsync(int id, int? driverId)
        {
            var car = await _repo.GetByIdAsync(id);
            if (car == null) return;
            car.DriverId = driverId;
            await _repo.UpdateAsync(car);
        }

        public async Task ToggleActiveAsync(int id)
        {
            var car = await _repo.GetByIdAsync(id);
            if (car == null) return;
            car.IsActive = !car.IsActive;
            await _repo.UpdateAsync(car);
        }

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
            _logger.LogInformation("Car {CarId} deleted", id);
        }
    }
}