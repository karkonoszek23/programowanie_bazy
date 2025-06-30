// Requests.cs
using System;
using System.Collections.Generic;

namespace Requests
{
    // Klasy dla żądań API (Request DTOs)
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Birthday { get; set; } = string.Empty; // Data jako string, do parsowania
        public string Gender { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty; // Changed from Login to Username
        public string Password { get; set; } = string.Empty;
    }

    public class CartUpdateItemRequest
    {
        public int UserId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; } = 1; // Domyślna ilość to 1
    }

    public class CartUserRequest
    {
        public int UserId { get; set; }
    }

    public class OrderPlaceRequest
    {
        public int UserId { get; set; }
    }

    // Klasy dla danych z bazy danych / odpowiedzi API (Response DTOs & Data Models)
    public class ShopItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }

    public class CartItem
    {
        public int ItemId { get; set; }
        public string ProductName { get; set; } = string.Empty; // Zmieniono Name na ProductName dla spójności
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalItemPrice { get; set; }
    }

    // Klasy, które były brakujące w Database.cs (z oryginalnego pliku)
    public class CartInfo
    {
        public int CartId { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
        public string Status { get; set; } = string.Empty; // Dodane, jeśli potrzebne
    }

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CartId { get; set; }
        public decimal TotalAmount { get; set; } // Dodano
        public DateTime OrderDate { get; set; } // Dodano
        public string Status { get; set; } = string.Empty;
    }

    public class OrderItem
    {
        public int ItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal TotalItemPrice { get; set; }
    }

    // Klasa odpowiedzi dla logowania
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; } // Nullable, jeśli logowanie nieudane
    }

    // Klasa odpowiedzi dla rejestracji
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
