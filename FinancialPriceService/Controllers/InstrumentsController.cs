using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;

namespace FinancialPriceService.Controllers
{

    public class InstrumentsController : ControllerBase
    {
        private readonly BinanceService binanceService;

        public InstrumentsController(BinanceService binanceService)
        {
            this.binanceService = binanceService;
        }

        private static readonly List<string> instruments = new List<string>
        {
            "BTCUSDT",
            "ETHUSDT",
            "BNBUSDT"
        };

        [HttpGet("instruments")]
        public IActionResult GetInstruments()
        {
            return Ok(instruments);
        }

        [HttpGet("{symbol}")]
        public IActionResult GetInstrumentPrice(string symbol)
        {
            // Simulate getting the current price
            // In a real scenario, fetch the latest price from BinanceService
            var price = binanceService.GetLatestPrice(symbol);
            if (price == null)
            {
                return NotFound();
            }
            return Ok(new { symbol, price });
        }
    }
}
