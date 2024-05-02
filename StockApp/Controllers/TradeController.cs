﻿using StockApp.ConfigurationOptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Models;
using ServicesContract;
using ServicesContract.DTO;
using Rotativa.AspNetCore;
using StockApp.Filters;

namespace StockApp.Controllers
{
    [Route("[Controller]")]
    public class TradeController : Controller
    {
        private readonly TradingOption _tradingOption;
        private readonly IFinnhubGetCompaniesDataService _finnhubGetCompaniesDataService;
        private readonly IFinnhubGetDetailsService _finnhubGetDetailsService;
        private readonly IStocksCreateBuyOrderService _stocksCreateBuyOrderService;
        private readonly IStocksCreateSellOrderService _stocksCreateSellOrderService;
        private readonly IStocksGetBuyOrdersService _stocksGetBuyOrdersService;
        private readonly IStocksGetSellOrdersService _stocksGetSellOrdersService;
        private readonly ILogger<TradeController> _logger;
        public TradeController(IOptions<TradingOption> tradingoption, IStocksCreateBuyOrderService stocksCreateBuyOrderService,IStocksCreateSellOrderService stocksCreateSellOrderService,IStocksGetBuyOrdersService stocksGetBuyOrdersService, IStocksGetSellOrdersService stocksGetSellOrdersService, ILogger<TradeController> logger, IFinnhubGetDetailsService finnhubGetDetailsService,IFinnhubGetCompaniesDataService finnhubGetCompaniesDataService)
        {
            _tradingOption = tradingoption.Value;
            _logger = logger;
            _finnhubGetDetailsService = finnhubGetDetailsService;
            _finnhubGetCompaniesDataService = finnhubGetCompaniesDataService;
            _stocksCreateBuyOrderService = stocksCreateBuyOrderService;
            _stocksCreateSellOrderService = stocksCreateSellOrderService;
            _stocksGetBuyOrdersService = stocksGetBuyOrdersService;
            _stocksGetSellOrdersService = stocksGetSellOrdersService;
        }
        [Route("/")]
        [Route("[Action]/{stockSymbol?}")]
        public async Task<IActionResult> Index(string stockSymbol)
        {
            _logger.LogInformation("{class}.{method} method",nameof(TradeController),nameof(Index));
            if(_tradingOption==null|| _tradingOption.DefaultStockSymbol==null)
            {
                throw new ArgumentNullException(nameof(_tradingOption));
            }
            else
            {
                if(stockSymbol==null)
                {
                    stockSymbol = _tradingOption.DefaultStockSymbol;
                }
                _logger.LogDebug($"stock symbol is : {stockSymbol}");
                _tradingOption.DefaultStockSymbol = stockSymbol;
                Dictionary<string, object>? companyProfile = await _finnhubGetDetailsService.GetCompanyProfile(stockSymbol ?? throw new ArgumentNullException("stock symbol can't be null"));
                Dictionary<string, object>? companyQuote = await _finnhubGetDetailsService.GetStockPriceQuote(stockSymbol);
                StockTrade stockTrade = new StockTrade()
                {
                    StockName = companyProfile?["name"].ToString(),
                    StockSymbol = companyProfile?["ticker"].ToString(),
                    Price = Convert.ToDouble(companyQuote?["c"].ToString()),
                    Quantity = _tradingOption.DefaultOrderQuantity
                };
                ViewBag.path = stockTrade.StockSymbol;
                ViewBag.dep = "Trade";
                //ViewBag.errors = ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage).ToList();
                return View(stockTrade);
            }
        }
        //Trade/BuyOrder
        [TypeFilter(typeof(CreateOrderActionFilter))]
        [Route("[action]")]
        public async Task<IActionResult> BuyOrder(BuyOrderRequest request)
        {
            await _stocksCreateBuyOrderService.CreateBuyOrder(request);
            return RedirectToAction("Orders");
        }
        [TypeFilter(typeof(CreateOrderActionFilter))]
        [Route("[action]")]
        public async Task<IActionResult> SellOrder(SellOrderRequest request)
        {
            await _stocksCreateSellOrderService.CreateSellOrder(request);
            return RedirectToAction("Orders");
        }
        [Route("[action]")]
        public async Task<IActionResult> Orders()
        {
            _logger.LogInformation("start orders");
            ViewBag.path = "Orders";
            ViewBag.dep = "Trade";
            var buyOrders = await _stocksGetBuyOrdersService.GetBuyOrders();
            var buyordersOrdered = buyOrders.OrderByDescending(x => x.DateAndTimeOfOrder).ToList();
            var sellOrders = await _stocksGetSellOrdersService.GetSellOrders();
            var sellOrdersOrdered = sellOrders.OrderByDescending(x => x.DateAndTimeOfOrder).ToList();
            Orders orders = new Orders() { BuyOrders = buyordersOrdered, SellOrders = sellOrdersOrdered };
            return View(orders);
        }
        [Route("[action]")]
        public async Task<IActionResult> OrdersPDF()
        {
            var buyOrders = await _stocksGetBuyOrdersService.GetBuyOrders();
            var sellorders = await _stocksGetSellOrdersService.GetSellOrders();
            Orders orders = new Orders() { BuyOrders = buyOrders, SellOrders = sellorders };
            return new ViewAsPdf(orders, ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins()
                {
                    Top = 20,
                    Right = 20,
                    Left = 20,
                    Bottom = 20
                },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }
    }
}
