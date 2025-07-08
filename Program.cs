using Requests;
using Validation;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();


app.MapPost("/api/login", async (LoginRequest req) =>
{
    UserLogin user = new UserLogin(req);
    DBConnection dbcontext = new DBConnection("admin", "admin");
    var fields = user.FetchFields();
    string login = fields[0];
    string passwd = fields[1];

    if (dbcontext.LoginUser(login, passwd))
    {
        Console.WriteLine($"Logowanie: {login}.");

        int userId = dbcontext.GetUserId(login);
        return Results.Ok(new
        {
            message = "Poprawnie zalogowano.",
            userId = userId
        });
    }
    else
    {
        Console.WriteLine($"Logowanie niepowiodlo sie: {login}\n{passwd}");
        return Results.Unauthorized();
    }
})
.WithName("Login")
.WithOpenApi();

app.MapPost("/api/register", async (RegisterRequest req) =>
{
    UserRegistration user = new UserRegistration(req);
    DBConnection dbcontext = new DBConnection("admin", "admin");
    var fields = user.FetchFields();
    string login = fields[0];
    string password = fields[1];
    string email = fields[2];
    string name = fields[3];
    string lastName = fields[4];
    string birthday = fields[5];
    string gender = fields[6];
    string phoneNumber = fields[7];
    string address = fields[8];
    Console.WriteLine($"Registration: {login} {password}");

    int validationResult = user.AmIGood();
    if (validationResult != 0)
    {
        string errorMessage = "Błąd rejestracji.";
        switch (validationResult)
        {
            case 1:
                errorMessage = "Login musi mieć więcej niż 6 znaków, a hasło więcej niż 8.";
                break;
            case 2:
                errorMessage = "Nieprawidłowy adres email.";
                break;
            case 3:
                errorMessage = "Wybrano niepoprawnie plec.";
                break;
            case 4:
                errorMessage = "Nieprawidłowy numer telefonu.";
                break;
        }
        return Results.BadRequest(new { message = errorMessage });
    }

    try
    {
        bool success = false;
        if (!dbcontext.LoginUser(login, password))
        {
            dbcontext.RegisterUser(user.FetchFields());
            success = true;
        }
        else
        {
            return Results.Conflict(new { message = "Użytkownik o podanym loginie lub emailu już istnieje." });
        }


        if (success)
        {
            Console.WriteLine($"Rejestracja użytkownika: {login}");
            return Results.Ok(new { message = "Rejestracja zakończona pomyslnie!" });
        }
        else
        {
            return Results.Conflict(new { message = "Użytkownik o podanej nazwie lub emailu już istnieje." });
        }
    }
    catch (MySql.Data.MySqlClient.MySqlException ex)
    {
        Console.Error.WriteLine($"Błąd bazy danych podczas rejestracji: {ex.Message}");
        if (ex.Number == 1062)
        {
            return Results.Conflict(new { message = "Użytkownik o podanym loginie lub emailu już istnieje." });
        }
        return Results.Problem(ex.Message);
    }
    catch (FormatException ex)
    {
        return Results.BadRequest(new { message = "Nieprawidłowy format daty urodzenia lub numeru telefonu." });
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"{ex.Message}");
        return Results.Problem(ex.Message);
    }
})
.WithName("Register")
.WithOpenApi();


app.MapGet("/api/shop/items", () =>
{
    try
    {
        var dbcontext = new DBConnection("admin", "admin");
        var items = dbcontext.GetShopItems();
        Console.WriteLine(items);
        return Results.Ok(items);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Błąd podczas pobierania przedmiotów sklepu: {ex.Message}");
        return Results.Problem(ex.Message);
    }
})
.WithName("GetShopItems")
.WithOpenApi();

app.MapGet("/api/cart", (int? userId) =>
{
    if (!userId.HasValue)
    {
        return Results.BadRequest(new { message = "Identyfikator użytkownika (userId) jest wymagany." });
    }

    try
    {

        var dbcontext = new DBConnection("admin", "admin");
        var cartItems = dbcontext.GetCartItems(userId.Value);
        return Results.Ok(cartItems);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Błąd podczas pobierania zawartości koszyka dla użytkownika {userId}: {ex.Message}");
        return Results.Problem(ex.Message);
    }
})
.WithName("GetCart")
.WithOpenApi();
app.MapPost("/api/cart/add", (CartUpdateItemRequest request) =>
{
    try
    {
        var dbcontext = new DBConnection("admin", "admin");
        var item = dbcontext.GetShopItemById(request.ItemId);
        if (item == null)
        {
            return Results.NotFound(new { message = "Produkt nie znaleziony." });
        }
        if (item.StockQuantity < request.Quantity)
        {
            return Results.BadRequest(new { message = $"Brak wystarczającej ilości produktu {item.Name} w magazynie. Dostępne: {item.StockQuantity}." });
        }

        var success = dbcontext.AddToCart(request.UserId, request.ItemId, request.Quantity);
        return success ? Results.Ok(new { message = "Przedmiot dodany do koszyka." }) : Results.BadRequest(new { message = "Nie udało się dodać przedmiotu do koszyka." });
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Błąd podczas dodawania przedmiotu do koszyka: {ex.Message}");
        return Results.Problem(ex.Message);
    }
})
.WithName("AddToCart")
.WithOpenApi();

app.MapPost("/api/cart/remove", (CartUpdateItemRequest request) =>
{
    try
    {
        var dbcontext = new DBConnection("admin", "admin");
        var success = dbcontext.RemoveFromCart(request.UserId, request.ItemId, request.Quantity);
        return success ? Results.Ok(new { message = "Przedmiot usunięty z koszyka." }) : Results.BadRequest(new { message = "Nie udało się usunąć przedmiotu z koszyka." });
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Błąd podczas usuwania przedmiotu z koszyka: {ex.Message}");
        return Results.Problem(ex.Message);
    }
})
.WithName("RemoveFromCart")
.WithOpenApi();

app.MapPost("/api/cart/clear", (CartUserRequest request) =>
{
    try
    {
        var dbcontext = new DBConnection("admin", "admin");
        var success = dbcontext.ClearCart(request.UserId);
        return success ? Results.Ok(new { message = "Koszyk został wyczyszczony." }) : Results.BadRequest(new { message = "Nie udało się wyczyścić koszyka." });
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Błąd podczas czyszczenia koszyka: {ex.Message}");
        return Results.Problem(ex.Message);
    }
})
.WithName("ClearCart")
.WithOpenApi();

app.MapPost("/api/orders/place", (OrderPlaceRequest request) =>
{
    try
    {
        var dbcontext = new DBConnection("admin", "admin");
        var cartItems = dbcontext.GetCartItems(request.UserId);

        if (cartItems == null || cartItems.Count == 0)
        {
            return Results.BadRequest(new { message = "Koszyk jest pusty. Nie można złożyć zamówienia." });
        }


        bool success = dbcontext.PlaceOrder(request.UserId);

        if (success)
        {
            return Results.Ok(new { message = "Zamówienie zostało złożone pomyślnie!" });
        }
        else
        {
            return Results.BadRequest(new { message = "Nie udało się złożyć zamówienia. Spróbuj ponownie." });
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Błąd podczas składania zamówienia: {ex.Message}");
        return Results.Problem(ex.Message);
    }
})
.WithName("PlaceOrder")
.WithOpenApi();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api") &&
        !context.Request.Path.StartsWithSegments("/api/login") &&
        !context.Request.Path.StartsWithSegments("/api/register"))
    {
        var userId = context.Request.Query["userId"].ToString();
        if (string.IsNullOrEmpty(userId))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: User ID required.");
            return;
        }
    }

    await next();
});


app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/index.html");
});

app.Run();
