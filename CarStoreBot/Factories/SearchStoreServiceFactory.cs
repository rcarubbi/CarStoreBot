using CarStoreBot.Integration.Interfaces;
using CarStoreBot.Integration.Interfaces.Fakes;
using CarStoreBot.Integration.Models;
using System.Collections.Generic;

namespace CarStoreBot.Factories
{
    public class SearchStoreServiceFactory
    {
        internal static Dictionary<int, Store> Stores = new Dictionary<int, Store>()
        {
            {
                1, new Store
                {
                    Id = 1,
                    Name = "Carrera Chevrolet - Villa Lobos",
                    Address = "Rua Henri Bouchard, 77 - Vila Leopoldina, São Paulo - SP, 05319-070",
                    Phones = "(11) 4002-1515",
                    Latitude = -23.5399084,
                    Longitude = -46.7303012
                }
            },
            {
                2, new Store
                {
                    Id = 2,
                    Name = "Chevrolet Palazzo",
                    Address = "R. Visc. de Nanique, 10 - Água Branca, São Paulo - SP, 02924-000",
                    Phones = "(11) 3612-6000",
                    Latitude = -23.5105667,
                    Longitude = -46.6915401
                }
            },
            {
                3, new Store
                {
                    Id = 3,
                    Name = "Nova Chevrolet - Peças Barra Funda",
                    Address = "Av. Marquês de São Vicente, 1584 - Várzea da Barra Funda, São Paulo - SP, 01139-002",
                    Phones = "(11) 3619-0800",
                    Latitude = -23.518865,
                    Longitude = -46.6779336
                }
            },
            {
                4, new Store
                {
                    Id = 4,
                    Name = "Chevrolet Itacolomy",
                    Address = "R. Heitor Penteado, 800 - Sumarezinho, São Paulo - SP, 05438-000",
                    Phones = "(11) 2103-9000",
                    Latitude = -23.547646,
                    Longitude = -46.6887177
                }
            },
            {
                5, new Store
                {
                    Id = 5,
                    Name = "Chevrolet Absoluta",
                    Address = "Av. Antártica, 62 - Jardim Industrial Tomas Edson, São Paulo - SP, 01141-060",
                    Phones = "(11) 4130-3931",
                    Latitude = -23.5220389,
                    Longitude = -46.6696132
                }
            },
        };

        public static ISearchStoreService Create()
        {
            return new StubISearchStoreService
            {
                GetByIdInt32 = (id) => Stores[id],
                SearchString = (postcode) => postcode.Replace("-", string.Empty) != "05116090" 
                    ? new List<Store>() 
                    : new List<Store> { Stores[1], Stores[2], Stores[3] },
                SearchStringString = (county, city) =>
                {
                    if (county.ToLower() != "sp" && county.ToLower() != "são paulo" && county.ToLower() != "sao paulo")
                        return new List<Store>();

                    return new List<Store> { Stores[1], Stores[2], Stores[3], Stores[4], Stores[5] }; 
                },
                SearchDoubleDouble = (latitude, longiture) => new List<Store> { Stores[1], Stores[2], Stores[3] }
            };

        }
    }
}