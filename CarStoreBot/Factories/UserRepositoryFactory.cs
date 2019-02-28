using CarStoreBot.Integration.Interfaces;
using CarStoreBot.Integration.Models;
using System.Collections.Generic;

namespace CarStoreBot.Factories
{
    public class UserRepositoryFactory
    {
        private static string[] emails = { "rcarubbi@gmail.com", "fribeiro@gmail.com", "mcamara@gmail.com", "ecarnieto@gmail.com", "rvieira@gmail.com" };
        private static Dictionary<string, User> _users = new Dictionary<string, User>
        {
            {emails[0], new User { Email = emails[0], Name = "Raphael Carubbi Neto", Gender = Genders.Male }},
            {emails[1], new User { Email = emails[1], Name = "Flávio Henrique Ribeiro", Gender = Genders.Male }},
            {emails[2], new User { Email = emails[2], Name = "Mario Mota Camara", Gender = Genders.Male }},
            {emails[3], new User { Email = emails[3], Name = "Ed Carlos Carnieto", Gender = Genders.Male }},
            {emails[4], new User { Email = emails[4], Name = "Rodrigo Riccitelli Vieira", Gender = Genders.Male }}
        };

        internal static IUserRepository CreateUserRepository()
        {
            return new CarStoreBot.Integration.Interfaces.Fakes.StubIUserRepository()
            {
                SearchString = (email) => _users.ContainsKey(email) ? _users[email] : null
            };
        }
    }
}