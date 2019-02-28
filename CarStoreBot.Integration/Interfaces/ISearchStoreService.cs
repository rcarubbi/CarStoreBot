using CarStoreBot.Integration.Models;
using System.Collections.Generic;

namespace CarStoreBot.Integration.Interfaces
{
    public interface ISearchStoreService
    {
        Store GetById(int id);
        List<Store> Search(string postcode);
        List<Store> Search(string county, string city);
        List<Store> Search(double latitude, double longitude);
    }
}
