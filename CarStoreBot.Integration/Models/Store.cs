using System;

namespace CarStoreBot.Integration.Models
{
    [Serializable]
    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string Phones { get; set; }
    }
}
