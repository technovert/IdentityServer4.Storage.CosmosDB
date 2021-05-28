# IdentityServer4.Storage.CosmosDB
CosmosDB persistence layer for .NetCore 3.1 IdentityServer4 based on the [IdentityServer4.Contrib.CosmosDB](https://github.com/jnhaffey/IdentityServer4.Contrib.CosmosDB) persistence layer.

## General Setup and Use

_appsettings.json_
```JSON
{
  "CosmosDb": {
    "EndPointUrl": "https://localhost:8081",
    "PrimaryKey": "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
    "DatabaseName": "MyTestDatabase",
    "Collections":[
      {
        "CollectionName": "ApiResources",
        "ReserveUnits": 1000
      }
    ]
  }
}
```
**EndPointUrl** and **PrimaryKey** are required values.  
**DatabaseName** and **Collections** are optional values.
Within Collections Objects, **CollectionName** can only be one of the following:  
• ApiResources    
• Clients  
• IdentityResources  
• PersistedGrants

_Startup.cs_
```CSharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    services.AddMvc()
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
        .AddJsonOptions(
            options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

    services.AddIdentityServer(options =>
        {
            options.Events.RaiseSuccessEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseErrorEvents = true;
        })
        .AddConfigurationStore(Configuration.GetSection("CosmosDB"))
        .AddOperationalStore(Configuration.GetSection("CosmosDB"))
        .AddDeveloperSigningCredential()
        .AddExtensionGrantValidator<ExtensionGrantValidator>()
        .AddExtensionGrantValidator<NoSubjectExtensionGrantValidator>()
        .AddJwtBearerClientAuthentication()
        .AddAppAuthRedirectUriValidator()
        .AddTestUsers(TestUsers.Users);
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
{
    if (env.IsDevelopment())
        app.UseDeveloperExceptionPage();
    else
        app.UseHsts();

    app.UseIdentityServer();
    app.UseIdentityServerCosmosDbTokenCleanup(applicationLifetime);

    app.UseStaticFiles();
    app.UseHttpsRedirection();
    app.UseMvcWithDefaultRoute();
}
```