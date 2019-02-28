using CarStoreBot.Integration.Models;

namespace CarStoreBot.Integration.Interfaces
{
    public interface IVechicleSearchService
    {
        Vehicle SearchByPlate(string plate);
        Vehicle SearchByChassis(string chassis);
    }
}
