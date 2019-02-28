using System.Collections.Generic;

namespace CarStoreBot.Integration.Models
{
    public class Warranty
    {
        public Warranty()
        {
            InsuredParts = new List<string>();
        }

        public string Title { get; set; }
        public List<string> InsuredParts { get; set; }

        public List<string> Diagnostics { get; set; }

        public string Guarantee { get; set; }
    }
}
