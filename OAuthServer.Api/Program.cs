using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OAuthServer.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Ensure this line is present

var configuration = builder.Configuration;
var apiScopesConfig = configuration.GetSection("IdentityServer:ApiScopes").Get<List<ApiScopeConfig>>();
var clientsConfig = configuration.GetSection("IdentityServer:Clients").Get<List<ClientConfig>>();

var apiScopes = apiScopesConfig.Select(s => new ApiScope(s.Name, s.DisplayName)).ToList();
var clients = clientsConfig.Select(c => new Client
{
    ClientId = c.ClientId,
    AllowedGrantTypes = c.AllowedGrantTypes,
    ClientSecrets = c.ClientSecrets.Select(s => new Secret(s.Sha256())).ToList(),
    AllowedScopes = c.AllowedScopes
}).ToList();

builder.Services.AddIdentityServer()
    .AddInMemoryApiScopes(apiScopes)
    .AddInMemoryClients(clients)
    .AddDeveloperSigningCredential();

// Configure authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:7108"; // URL of your IdentityServer
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

// Configure authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1");
    });
});

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "OAuthServer.Api", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Ensure this line is present

app.Run();
