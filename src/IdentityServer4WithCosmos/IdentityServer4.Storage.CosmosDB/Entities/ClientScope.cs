using Newtonsoft.Json;

namespace IdentityServer4.Storage.CosmosDB.Entities
{
    /// <summary>
    ///     Instance of Client Scope.
    /// </summary>
    public class ClientScope
    {
        /// <summary>
        ///     Scope Value.
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }
    }
}