using System.Data.Entity.Infrastructure;

namespace CarStoreBot.State
{
    public class SqlBotDataContextFactory : IDbContextFactory<SqlBotDataContext>
    {
        public SqlBotDataContext Create()
        {
            return new SqlBotDataContext("BotDataContextConnectionString");
        }
    }
}