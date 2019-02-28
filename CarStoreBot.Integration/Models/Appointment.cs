using System;

namespace CarStoreBot.Integration.Models
{
    [Serializable]
    public class Appointment
    {
        public DateTime DateTime { get; set; }
        public Store Store { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}
