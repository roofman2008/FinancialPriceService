using Microsoft.AspNetCore.SignalR;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class BinanceService
{
    private readonly IHubContext<PriceHub> _hubContext;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private ConcurrentDictionary<string, decimal> _latestPrices = new ConcurrentDictionary<string, decimal>();
    private ConcurrentDictionary<string, decimal> _prevPrices = new ConcurrentDictionary<string, decimal>();
    private readonly List<string> instruments = new List<string>
    {
        "btcusdt",
        "ethusdt",
        "bnbusdt"
    };

    // Dictionary to hold WebSocket clients for each instrument
    private readonly Dictionary<string, ClientWebSocket> _clients = new Dictionary<string, ClientWebSocket>();

    public BinanceService(IHubContext<PriceHub> hubContext)
    {
        _hubContext = hubContext;
        ConnectAndSubscribe();
    }

    public decimal? GetLatestPrice(string symbol)
    {
        if (_latestPrices.TryGetValue(symbol.ToLower(), out var price))
        {
            return price;
        }
        return null;
    }

    private void ConnectAndSubscribe()
    {
        foreach (var instrument in instruments)
        {
            var client = new ClientWebSocket();
            _clients[instrument] = client;
            ConnectToInstrument(client, instrument);
        }
    }

    private async void ConnectToInstrument(ClientWebSocket client, string instrument)
    {
        try
        {
            // Connect to the combined stream endpoint
            var uri = new Uri($"wss://stream.binance.com:443/ws/{instrument}");
            await client.ConnectAsync(uri, _cts.Token);

            // Prepare subscription message
            var subscribeMessage = new
            {
                method = "SUBSCRIBE",
                @params = new[] { $"{instrument}@aggTrade" },
                id = 1
            };

            var messageJson = JsonSerializer.Serialize(subscribeMessage);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);

            // Send subscription message
            await client.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, _cts.Token);

            // Start receiving messages for this instrument
            _ = Task.Run(() => ReceiveMessages(client, instrument));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to {instrument}: {ex.Message}");
        }
    }

    private async Task ReceiveMessages(ClientWebSocket client, string instrument)
    {
        var buffer = new byte[1024 * 4];
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    Console.WriteLine($"Connection closed for {instrument}");
                    break;
                }
                else
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Deserialize the message into a JsonDocument to check for errors or subscription status
                    using var document = JsonDocument.Parse(json);
                    if (document.RootElement.TryGetProperty("e", out _)) // Check if it's an event message
                    {
                        var data = JsonSerializer.Deserialize<BinanceAggTrade>(json);
                        if (data != null)
                        {
                            var symbol = data.s.ToLower();
                            var currentPrice = decimal.Parse(data.p);

                            // Check if the price has changed
                            if (_latestPrices.TryGetValue(symbol, out var previousPrice))
                            {
                                if (currentPrice != previousPrice)
                                {
                                    // Update the latest price
                                    _latestPrices[symbol] = currentPrice;

                                    // Broadcast to subscribed clients
                                    await _hubContext.Clients.All.SendAsync("ReceivePriceUpdate", new { symbol = data.s, price = data.p });
                                }
                            }
                            else
                            {
                                // First time receiving price for this symbol
                                _latestPrices[symbol] = currentPrice;

                                // Broadcast to subscribed clients
                                await _hubContext.Clients.All.SendAsync("ReceivePriceUpdate", new { symbol = data.s, price = data.p });
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Received non-event message: {json}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data for {instrument}: {ex.Message}");
            }
        }
    }
}

public class BinanceAggTrade
{
    public string e { get; set; }
    public long E { get; set; }
    public string s { get; set; }
    public long a { get; set; }
    public string p { get; set; }
    public string q { get; set; }
    public long f { get; set; }
    public long l { get; set; }
    public long T { get; set; }
    public bool m { get; set; }
    public bool M { get; set; }
}
