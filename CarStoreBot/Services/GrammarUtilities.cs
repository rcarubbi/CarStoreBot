using CarStoreBot.Integration.Models;

namespace CarStoreBot.Services
{
    public static class GrammarUtilities
    {
        public static string FirstWord(this string phrase)
        {
            return phrase.Split(' ')[0];
        }


        public static string GenderInflection(this string word, User user)
        {
            if (user.Gender == Genders.Male)
                return word;
            else
            {
                return word == "Mr." ? "Mrs." : word;
            }
        }
    }
}