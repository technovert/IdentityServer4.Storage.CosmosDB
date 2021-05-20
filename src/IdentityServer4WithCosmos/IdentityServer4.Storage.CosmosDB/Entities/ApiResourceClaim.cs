using System.Collections.Generic;

namespace IdentityServer4.Storage.CosmosDB.Entities
{
    /// <inheritdoc />
    /// <summary>
    ///     Instance of API Resource Claim (User Claim).
    /// </summary>
    public class ApiResourceClaim : UserClaim
    {
    }

    public class ApiResourceClaim2 : Dictionary<int, string>
    {

    }
}