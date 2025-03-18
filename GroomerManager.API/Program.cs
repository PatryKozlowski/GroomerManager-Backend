using GroomerManager.API.Auth;
using GroomerManager.API.Exception;
using GroomerManager.Application;
using GroomerManager.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
const string CORS_POLICY = "GroomeManagerPolicy";
const string CONFIGRURATION_FRONTEND_NAME = "frontend";
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.local.Development.json");
}

// Add services to the container.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

builder.Services.AddCors(cors => cors.AddPolicy(CORS_POLICY, policy =>
{
    policy.WithOrigins(builder.Configuration.GetValue<string>(CONFIGRURATION_FRONTEND_NAME) ?? throw new InvalidOperationException("frontend url should be provided"));
    policy.WithMethods("POST", "PUT", "PATCH", "DELETE");
    policy.WithHeaders("content-type");
    // policy.WithHeaders("X-XSRF-TOKEN");
    // policy.WithHeaders("Bearer")
    // policy.AllowAnyHeader();
    policy.AllowCredentials();
}));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
var app = builder.Build();

app.UseCors(CORS_POLICY);

app.UseExceptionHandler(_ => { });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "GroomerManager Demo API");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();