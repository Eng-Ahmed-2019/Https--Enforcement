var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//var apiKey = builder.Configuration.GetValue<string>("ApiKey");
var apiKey = builder.Configuration["ExternalApi:ApiKey"];
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}
if (string.IsNullOrWhiteSpace(apiKey))
{
    throw new InvalidOperationException("API key 'ApiKey' not found.");
}

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7213;
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
});

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(30);
    options.IncludeSubDomains = false; // dev
    // options.IncludeSubDomains = true; // prod
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/user", () =>
{
    return Results.Ok(new { UserId = 15, UserName = "Ahmed", Message = "Secure Connection" });
});
app.MapGet("/secret", () =>
{
    return Results.Ok(new
    {
        Message = "Secrets loaded successfully!",

        DatabaseConfigured = string.IsNullOrWhiteSpace(connectionString)
                            ? "Empty" : connectionString,

        ApiKeyConfigured = string.IsNullOrWhiteSpace(apiKey)
                            ? "Empty" : apiKey
    });
});
app.MapControllers();
app.Run();