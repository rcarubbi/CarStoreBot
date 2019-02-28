using System;
using System.Collections.Generic;

namespace CarStoreBot.Integration.Models
{
    [Serializable]
    public class Vehicle
    {
        public Vehicle()
        {
            Warranties = new List<Warranty>();
        }

        public string Chassis { get; set; }

        public List<Warranty> Warranties { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string Plate { get; set; }
    }
}
