using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
public class PriceHub : Hub
{
    public async Task Subscribe(string instrument)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, instrument);
        Console.WriteLine($"Client subscribed to {instrument}");
    }

    public async Task Unsubscribe(string instrument)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, instrument);
        Console.WriteLine($"Client unsubscribed from {instrument}");
    }
}
