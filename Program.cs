using Validation;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapPost("/api/login", async (LoginRequest req) =>
{
    UserLogin user = new UserLogin(req);
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
public record RegisterRequest(string Username, string Email, string Password, string Name, string LastName, string Birthday, string Gender, string PhoneNumber, string Address);
