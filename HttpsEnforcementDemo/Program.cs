using Microsoft.AspNetCore.Antiforgery;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            //.WithOrigins("*")
            .WithOrigins("https://localhost:5173", "https://myfrontend.com")
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type", "Authorization");
            //.AllowCredentials().AllowAnyOrigin(); // not secure
    });
});

// if proj work with cookies
/*
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
*/

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

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-XSRF-TOKEN";
});

/*
builder.Services
    .AddAuthentication()
    .AddCookie(options =>
    {
        // lax is: 	يسمح ببعض حالات التنقل الآمنة، ويمنع أغلب الطلبات Cross-Site
        // strict: 	يمنع إرسال Cookie في أغلب سياقات التنقل Cross-Site
        // none: 	يسمح بإرسال Cookie في سياقات Cross-Site، ويتطلب Secure

        options.Cookie.HttpOnly = true;

        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;

        options.Cookie.SameSite =
            SameSiteMode.Lax;
    });
*/

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

app.MapGet("/api/User/GetUsers", () =>
{
    return Results.Ok(new { UserId = 15, UserName = "Ahmed", Message = "Secure Connection" });
});
app.MapGet("/api/Secret/GetData", () =>
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
app.MapGet("/api/AntiForgery/CreateToken", (
    IAntiforgery antiforgery,
    HttpContext context
) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);

    return Results.Ok(new
    {
        Token = tokens.RequestToken
    });
});
app.MapPost("/api/Transfer/TransMoney", async (
    HttpContext context,
    IAntiforgery antiforgery) =>
{
    try
    {
        await antiforgery.ValidateRequestAsync(context);

        return Results.Ok(new
        {
            Message = "Transfer completed successfully"
        });
    }
    catch (AntiforgeryValidationException)
    {
        return Results.BadRequest(new
        {
            Message = "Invalid Anti-Forgery Token"
        });
    }
});
app.UseStaticFiles();
app.UseRouting();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();