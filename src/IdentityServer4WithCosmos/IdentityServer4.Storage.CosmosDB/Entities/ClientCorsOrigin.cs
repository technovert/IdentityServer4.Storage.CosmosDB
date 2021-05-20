using Newtonsoft.Json;

namespace IdentityServer4.Storage.CosmosDB.Entities
{
    /// <summary>
    ///     Instance of Client CORS Origins.
    /// </summary>
    public class ClientCorsOrigin
    {
        /// <summary>
        ///     Origin Value.
        /// </summary>
        [JsonProperty("origin")]
        public string Origin { get; set; }
    }
}