
# Financial Instrument Price Service

This project provides a real-time financial instrument price tracking service with REST API and WebSocket capabilities. It retrieves price data from Binance and broadcasts updates to connected clients using SignalR.

## Features

- **REST API**: Fetch the list of available instruments and get the latest price for a specific instrument.
- **WebSocket**: Real-time price updates via WebSocket subscription to multiple instruments.
- **Data Source**: Live data from Binance's WebSocket API.
- **Scalable**: Designed to efficiently handle 1,000+ concurrent WebSocket subscribers.

## Technology Stack

- **Backend**: .NET Core, SignalR, WebSocket for real-time data
- **Frontend**: SignalR client for receiving updates
- **External API**: Binance WebSocket API for instrument prices

## Setup Instructions

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/financial-instrument-price-service.git
   cd financial-instrument-price-service
   ```

2. Open the solution in Visual Studio or VS Code.

3. **Configure Instruments** (Optional):
   - Update the list of instruments in `BinanceService` if additional instruments are needed:
     ```csharp
     private readonly List<string> instruments = new List<string> { "btcusdt", "ethusdt", "bnbusdt" };
     ```

4. **Run the Application**:
   - Start the project (both client and server applications).
   - The server connects to Binance’s WebSocket API and broadcasts price updates to all clients subscribed to specific instruments.

5. **API Endpoints**:
   - **GET /api/instruments**: List available financial instruments.
   - **GET /api/instruments/{symbol}/price**: Get the current price of a specific instrument.

## Usage

- **REST API**: Use the provided endpoints to access current prices.
- **WebSocket**: Subscribe to live price updates through SignalR to receive price updates in real-time.

## Example

After running the application, you can:

1. Access the list of available instruments:
   ```bash
   curl http://localhost:7075/api/instruments
   ```

2. Get the latest price for an instrument (e.g., BTCUSDT):
   ```bash
   curl http://localhost:7075/api/instruments/btcusdt/price
   ```
