﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.SqlDbToCosmosMigrator
{
    public interface ISqlToCosmosMigratorContract
    {
        bool Migrate();
    }
}
