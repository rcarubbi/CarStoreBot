using System;

namespace CarStoreBot.Integration.Models
{
    [Serializable]

    public class Ticket
    {
        public Vehicle Vehicle { get; set; }
        public Store Store { get; set; }
        public string Status { get; set; }
        public DateTime FinishDate { get; set; }
    }
}
