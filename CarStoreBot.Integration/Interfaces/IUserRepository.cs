using CarStoreBot.Integration.Models;

namespace CarStoreBot.Integration.Interfaces
{
    public interface IUserRepository
    {
        User Search(string email);
    }
}
