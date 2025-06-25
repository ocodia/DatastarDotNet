using Microsoft.AspNetCore.Mvc;
using StarFederation.Datastar.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datastar.Controllers
{
    public class HomeController : Controller
    {
        private IServiceProvider _services;
        public HomeController(IServiceProvider services)
        {
            _services = services;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MultiStepForm()
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
              <div id='date' role="group">             
                <input type="text" value="{today}"/>
                <button class="secondary" data-on-click="@get('/removedate')">Remove</button>
              </div>
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
            Signals newSignals = new() { Output = $"{signals.Input}" };
            await sse.MergeSignalsAsync(newSignals.Serialize());
        }
    }
}
