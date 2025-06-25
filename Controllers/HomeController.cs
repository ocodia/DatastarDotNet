using Datastar.Models;
using Microsoft.AspNetCore.Mvc;
using StarFederation.Datastar.DependencyInjection;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datastar.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IServiceProvider _services;
        public HomeController(ILogger<HomeController> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("displayDate")]
        public async Task DisplayDate()
        {
            string today = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
            var _sse = _services.GetRequiredService<IDatastarServerSentEventService>();

            // push the fragment back over SSE
            await _sse.MergeFragmentsAsync($"""
            <div id='target'>
              <span id='date'>
                <b>{today}</b>
                <button class="btn btn-danger" data-on-click="@get('/removedate')">Remove</button>
              </span>
            </div>
            """);
        }

        [HttpGet("removedate")]
        public async Task RemoveDate()
        {
            var _sse = _services.GetRequiredService<IDatastarServerSentEventService>();

            await _sse.RemoveFragmentsAsync("#date");
        }

        public record Signals
        {
            [JsonPropertyName("input")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Input { get; init; } = null;

            [JsonPropertyName("output")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Output { get; init; } = null;

            public string Serialize() => JsonSerializer.Serialize(this);
        }

        [HttpPost("changeOutput")]
        public async Task ChangeOutput()
        {
            var sse = _services.GetRequiredService<IDatastarServerSentEventService>();
            var dsSignals = _services.GetRequiredService<IDatastarSignalsReaderService>();

            Signals signals = await dsSignals.ReadSignalsAsync<Signals>();
            Signals newSignals = new() { Output = $"Your Input: {signals.Input}" };
            await sse.MergeSignalsAsync(newSignals.Serialize());
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
