using Validation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();

// /api/login do requestow
app.MapPost("/api/login", async (LoginRequest req) =>
{
    FetchUser user = new FetchUser(req.Username, req.Password);
    if (user.AmIGood())
    {
        return Results.Ok(new { message = "Logowanie udane!", token = "jakis_token_jwt" });
    }
    else
    {
        return Results.Unauthorized();
    }
})
.WithName("Login")
.WithOpenApi();

app.MapPost("/api/register", async (RegisterRequest request) =>
{
    Console.WriteLine($"Próba rejestracji: Użytkownik={request.Username}, Email={request.Email}, Hasło={request.Password}");

    if (request.Username == "existinguser")
    {
        return Results.Conflict(new { message = "Użytkownik o tej nazwie już istnieje." });
    }
    return Results.Created("/api/register", new { message = "Rejestracja udana!" });
})
.WithName("Register")
.WithOpenApi();

app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/index.html");
});


app.Run();
public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Email, string Password);
