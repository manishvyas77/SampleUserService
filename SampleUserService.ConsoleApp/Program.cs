using Microsoft.Extensions.Hosting;
using SampleUserService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleUserService.Extensions;

Console.WriteLine("Hello, World!");
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddExternalUserService(context.Configuration);
    })
    .Build();

var userService = builder.Services.GetRequiredService<IExternalUserService>();

try
{
    Console.WriteLine("Fetching all users...");
    var allUsers = await userService.GetAllUsersAsync();
    Console.WriteLine($"Found {allUsers.Count()} users:");
    
    foreach (var user in allUsers)
    {
        Console.WriteLine($"- {user.FirstName} {user.LastName} ({user.Email})");
    }

    Console.WriteLine("\nFetching user with ID 1...");
    var singleUser = await userService.GetUserByIdAsync(1);
    if (singleUser != null)
    {
        Console.WriteLine($"Found user: {singleUser.FirstName} {singleUser.LastName}");
    }
    else
    {
        Console.WriteLine("User not found");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}