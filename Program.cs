using Validation;
using Database;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapPost("/api/login", async (LoginRequest req) =>
{
    UserLogin user = new UserLogin(req);

    // +-------------------------------------------------------+
    // | CREATE USER 'admin'@'localhost' IDENTIFIED BY 'admin';|
    // | GRANT ALL PRIVILEGES ON shop.* TO 'admin'@'localhost';| 
    // +-------------------------------------------------------+ 

    DBConnection dbcontext = new DBConnection("admin", "admin");
    var fields = user.FetchFields();
    string login = fields[0];
    string passwd = fields[1];
    if (dbcontext.LoginUser(login, passwd))
    {
        Console.WriteLine("Zalogowano uzytkownika.");
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
    if (request is null)
    {
        return Results.Unauthorized();
    }
    else
    {
        Console.WriteLine(request);
    }
    UserRegistration tempUser = new UserRegistration(request);
    if (tempUser.AmIGood() == 0)
    {
        DBConnection dbcontext = new DBConnection("admin", "admin");
        dbcontext.RegisterUser(tempUser.FetchFields());
        Console.WriteLine("Zarejestrowano uzytkownika.");
        return Results.Created("/api/register", new { message = "Rejestracja udana!" });
    }
    else
    {
        Console.WriteLine($"Kod bledu: {tempUser.AmIGood()}");
    }
    return Results.Unauthorized();
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
