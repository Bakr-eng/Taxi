using Microsoft.Extensions.Logging;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;

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

        public async Task<List<Car>> GetAllAsync()
            => await _repo.GetAllAsync();

        public async Task<Car?> GetByIdAsync(int id)
            => await _repo.GetByIdAsync(id);

        public async Task CreateAsync(string registrationNumber, string make, string model, int year, int seats, int? driverId = null)
        {
            var car = new Car
            {
                RegistrationNumber = registrationNumber,
                Make = make,
                Model = model,
                Year = year,
                Seats = seats,
                DriverId = driverId,
                IsActive = true
            };
            await _repo.AddAsync(car);
            _logger.LogInformation("Car {RegNr} created", registrationNumber);
        }

        public async Task UpdateAsync(Car car)
            => await _repo.UpdateAsync(car);

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
            _logger.LogInformation("Car {CarId} deleted", id);
        }
    }
}
