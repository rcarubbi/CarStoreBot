using System;

namespace CarStoreBot.Integration.Models
{
    [Serializable]
    public class User
    {
        public string Name { get; set; }
        public Genders Gender { get; set; }
        public string Email { get; set; }
    }
}