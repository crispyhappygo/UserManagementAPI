using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1. Global Exception Handling Middleware (Prevents server crashes and unhandled exceptions)
/* app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new 
        { 
            message = "An unexpected error occurred on the server. Please try again later." 
        });
    });
}); */

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseMiddleware<TokenAuthenticationMiddleware>();

app.UseMiddleware<RequestLoggingMiddleware>();

// In-memory dictionary storage
var users = new Dictionary<int, User>
{
    { 1, new User { Id = 1, Username = "asmith", Age = 30, Department = "HR" } },
    { 2, new User { Id = 2, Username = "bjones", Age = 25, Department = "IT" } }
};

int nextId = 3;

// --- ENDPOINTS WITH ROBUST VALIDATION & ERROR HANDLING ---

// 1. GET: Retrieve all users or filter by search query parameter
app.MapGet("/api/users", (string? search) =>
{
    var query = users.Values.AsEnumerable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(u => 
            u.Username.Contains(search, StringComparison.OrdinalIgnoreCase) || 
            u.Department.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    return Results.Ok(query.ToList());
});

// 2. GET: Retrieve a specific user by ID
app.MapGet("/api/users/{id:int}", (int id) =>
{
    if (users.TryGetValue(id, out var user))
    {
        return Results.Ok(user);
    }
    return Results.NotFound(new { message = $"User with ID {id} not found." });
});

// 3. POST: Add a new user with strict validation
app.MapPost("/api/users", (User newUser) =>
{
    // Input Validation checks
    if (string.IsNullOrWhiteSpace(newUser.Username))
    {
        return Results.BadRequest(new { message = "Username is required and cannot be empty." });
    }
    if (newUser.Age <= 0 || newUser.Age > 120)
    {
        return Results.BadRequest(new { message = "Age must be a valid number between 1 and 120." });
    }
    if (string.IsNullOrWhiteSpace(newUser.Department))
    {
        return Results.BadRequest(new { message = "Department is required." });
    }

    newUser.Id = nextId++;
    users[newUser.Id] = newUser;

    return Results.Created($"/api/users/{newUser.Id}", newUser);
});

// 4. PUT: Update an existing user with validation
app.MapPut("/api/users/{id:int}", (int id, User updatedUser) =>
{
    if (!users.ContainsKey(id))
    {
        return Results.NotFound(new { message = $"User with ID {id} not found." });
    }

    if (string.IsNullOrWhiteSpace(updatedUser.Username) || string.IsNullOrWhiteSpace(updatedUser.Department))
    {
        return Results.BadRequest(new { message = "Username and Department cannot be empty." });
    }

    updatedUser.Id = id; // Ensure ID consistency
    users[id] = updatedUser;

    return Results.NoContent();
});

// 5. DELETE: Remove a user by ID
app.MapDelete("/api/users/{id:int}", (int id) =>
{
    if (users.Remove(id))
    {
        return Results.NoContent();
    }
    return Results.NotFound(new { message = $"User with ID {id} not found." });
});

// Test endpoint to trigger unhandled exception for ErrorHandlingMiddleware
app.MapGet("/api/error-test", () =>
{
    throw new Exception("Simulated server exception!");
});

app.Run();


/* Assistance Documentation 

(for activity 1)
1. Robust Error Handling & Validation: AI recommended adding data checks (e.g., verifying that mandatory fields like Name and Email are not null or empty) and returning standard HTTP status codes (BadRequest, NotFound, CreatedAtAction, NoContent) rather than relying on default unhandled exceptions.

2. Streamlined Testing Workflow: AI guided the integration of the VS Code REST Client extension and .http script files, removing the dependency on external applications like Postman and keeping the developer workflow entirely inside VS Code.

3. RESTful Best Practices: AI helped structure route attributes and return types accurately adhering to standard ASP.NET Core conventions (api/[controller], action results, and proper REST verbs).

(for activity 2)
1.Global Exception Boundaries: AI introduced the UseExceptionHandler middleware pipeline to intercept unhandled runtime errors, replacing raw server crash stacks with predictable, formatted JSON error payloads (500 Internal Server Error).

2.Input Validation Guards: AI added boundary checking on incoming payload fields (Username, Age, Department) to protect the in-memory dictionary state from malformed or incomplete data entries (400 Bad Request).

3.Defensive Error Codes: Ensured exact semantic REST mapping (404 NotFound for missing dictionary lookups instead of null reference exceptions). */