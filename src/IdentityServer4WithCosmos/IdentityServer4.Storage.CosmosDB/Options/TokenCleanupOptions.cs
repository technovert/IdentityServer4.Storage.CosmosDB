namespace IdentityServer4.Storage.CosmosDB.Options
{
    public class TokenCleanupOptions
    {
        public int Interval { get; set; } = 60;
    }
}