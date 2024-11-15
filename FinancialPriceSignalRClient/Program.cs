using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            
            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7075/pricehub") // Adjust the URL to your hub
                .Build();

            // Set up the handler for "ReceivePriceUpdate"
            connection.On<JsonElement>("ReceivePriceUpdate", jsonElement =>
            {
                string symbol = jsonElement.GetProperty("symbol").GetString();
                decimal price = decimal.Parse(jsonElement.GetProperty("price").GetString());
                Console.WriteLine($"Price update for {symbol}: {price}");
            });

            // Start the connection
            await connection.StartAsync();
            Console.WriteLine("Connected to the hub");

            // Keep the console open
            Console.ReadLine();
        }
    }
}
