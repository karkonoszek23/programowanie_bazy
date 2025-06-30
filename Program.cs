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
        return Results.Ok(new { message = "Logowanie udane!", token = "jakis_token_jwt", userId = 123 }); // Przykładowe userId
    }
    else
    {
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

    int validationResult = user.AmIGood();
    if (validationResult != 0)
    {
        string errorMessage = "Nieznany błąd rejestracji.";
        switch (validationResult)
        {
            case 1:
                errorMessage = "Login musi mieć więcej niż 6 znaków, a hasło więcej niż 8.";
                break;
            case 2:
                errorMessage = "Nieprawidłowy format adresu email.";
                break;
            case 3:
                errorMessage = "Nieprawidłowa płeć. Dozwolone: M, K, N.";
                break;
            case 4:
                errorMessage = "Nieprawidłowy numer telefonu (musi mieć 9 cyfr).";
                break;
        }
        return Results.BadRequest(new { message = errorMessage });
    }

    try
    {
        // POPRAWKA: Przekazanie tablicy string[] do RegisterUser
        bool success = false;
        // Sprawdź czy użytkownik już istnieje przed próbą rejestracji
        if (!dbcontext.LoginUser(login, password)) // Proste sprawdzenie, czy login nie istnieje (zakładając, że LoginUser zwraca false dla nieistniejących)
        {
            // W Database.cs RegisterUser nie zwraca bool, rzuca wyjątki.
            // Zmieniam wywołanie, aby obsłużyć wyjątki i zwracać odpowiedni Results.
            dbcontext.RegisterUser(user.FetchFields());
            success = true;
        }
        else
        {
            return Results.Conflict(new { message = "Użytkownik o podanym loginie lub emailu już istnieje." });
        }


        if (success)
        {
            Console.WriteLine($"Zarejestrowano użytkownika: {login}");
            return Results.Ok(new { message = "Rejestracja zakończona sukcesem!" });
        }
        else
        {
            return Results.Conflict(new { message = "Użytkownik o podanej nazwie lub emailu już istnieje." });
        }
    }
    catch (MySql.Data.MySqlClient.MySqlException ex)
    {
        Console.Error.WriteLine($"Błąd bazy danych podczas rejestracji: {ex.Message}");
        // W zależności od typu błędu MySqlException, możesz zwrócić bardziej specyficzną wiadomość.
        // Np. Duplicate entry dla unikalnych kluczy.
        if (ex.Number == 1062) // MySQL error code for duplicate entry
        {
            return Results.Conflict(new { message = "Użytkownik o podanym loginie lub emailu już istnieje." });
        }
        return Results.Problem(ex.Message);
    }
    catch (FormatException ex)
    {
        Console.Error.WriteLine($"Błąd formatu danych podczas rejestracji użytkownika: {ex.Message}. Sprawdź format daty urodzenia i numeru telefonu.");
        return Results.BadRequest(new { message = "Nieprawidłowy format daty urodzenia lub numeru telefonu." });
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Błąd podczas rejestracji: {ex.Message}");
        return Results.Problem(ex.Message);
    }
})
.WithName("Register")
.WithOpenApi();

// Inicjalizacja DBConnection (dla GetShopItems)
// Ważne: W rzeczywistej aplikacji powinieneś używać wstrzykiwania zależności dla DBConnection,
// a nie tworzyć nowej instancji za każdym razem.
var database = new DBConnection("admin", "admin");

// GET: /api/shop/items - Pobieranie wszystkich produktów w sklepie
app.MapGet("/api/shop/items", () =>
{
    try
    {
        var items = database.GetShopItems();
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

// GET: /api/cart - Pobieranie zawartości koszyka dla danego użytkownika
// GET: /api/cart - Pobieranie zawartości koszyka dla danego użytkownika
app.MapGet("/api/cart", (int? userId) => // Zmieniono z 'int userId' na 'int? userId'
{
    // Dodaj sprawdzenie, czy userId ma wartość
    if (!userId.HasValue)
    {
        // Możesz zwrócić błąd Bad Request lub inny odpowiedni status
        return Results.BadRequest(new { message = "Identyfikator użytkownika (userId) jest wymagany." });
    }

    try
    {
        // Użyj userId.Value, aby uzyskać rzeczywistą wartość int
        var cartItems = database.GetCartItems(userId.Value);
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
// POST: /api/cart/add - Dodawanie przedmiotu do koszyka
app.MapPost("/api/cart/add", (CartUpdateItemRequest request) =>
{
    try
    {
        // Sprawdź, czy produkt istnieje i jest dostępny
        var item = database.GetShopItemById(request.ItemId);
        if (item == null)
        {
            return Results.NotFound(new { message = "Produkt nie znaleziony." });
        }
        if (item.StockQuantity < request.Quantity)
        {
            return Results.BadRequest(new { message = $"Brak wystarczającej ilości produktu {item.Name} w magazynie. Dostępne: {item.StockQuantity}." });
        }

        var success = database.AddToCart(request.UserId, request.ItemId, request.Quantity);
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

// POST: /api/cart/remove - Usuwanie przedmiotu z koszyka (lub zmniejszanie ilości)
app.MapPost("/api/cart/remove", (CartUpdateItemRequest request) =>
{
    try
    {
        var db = new DBConnection("admin", "admin"); // Użyj istniejącego połączenia lub wstrzykiwania zależności
        // W zależności od logiki biznesowej, możesz pozwolić na usunięcie określonej ilości lub całego przedmiotu
        var success = db.RemoveFromCart(request.UserId, request.ItemId, request.Quantity); // Zakładam, że ta metoda obsługuje ilość
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

// POST: /api/cart/clear - Czyszczenie całego koszyka
app.MapPost("/api/cart/clear", (CartUserRequest request) =>
{
    try
    {
        var db = new DBConnection("admin", "admin"); // Użyj istniejącego połączenia lub wstrzykiwania zależności
        var success = db.ClearCart(request.UserId);
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

// POST: /api/orders/place - Składanie zamówienia
app.MapPost("/api/orders/place", (OrderPlaceRequest request) =>
{
    try
    {
        var db = new DBConnection("admin", "admin"); // Użyj istniejącego połączenia lub wstrzykiwania zależności
        var cartItems = db.GetCartItems(request.UserId);

        if (cartItems == null || cartItems.Count == 0)
        {
            return Results.BadRequest(new { message = "Koszyk jest pusty. Nie można złożyć zamówienia." });
        }

        // Możesz dodać logikę weryfikacji dostępności produktów przed złożeniem zamówienia
        // oraz aktualizację stanu magazynowego po złożeniu zamówienia.

        // POPRAWKA: Zmiana PlaceOrderFromCart na PlaceOrder
        bool success = db.PlaceOrder(request.UserId); // Ta metoda powinna przenieść przedmioty z koszyka do zamówienia i wyczyścić koszyk

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

// Middleware do sprawdzania autoryzacji (prosty przykład, w rzeczywistej aplikacji użyj JWT)
app.Use(async (context, next) =>
{
    // Jeśli ścieżka zaczyna się od /api/, ale nie jest /api/login ani /api/register
    if (context.Request.Path.StartsWithSegments("/api") &&
        !context.Request.Path.StartsWithSegments("/api/login") &&
        !context.Request.Path.StartsWithSegments("/api/register"))
    {
        // Sprawdź, czy userId jest w parametrach zapytania dla innych endpointów API
        var userId = context.Request.Query["userId"].ToString();
        if (string.IsNullOrEmpty(userId))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Unauthorized: User ID required.");
            return;
        }
        // W bardziej zaawansowanych scenariuszach tutaj byłaby walidacja tokena JWT
    }

    await next();
});


app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/index.html");
});


app.Run();
