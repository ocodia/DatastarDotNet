using Datastar.Models;
using Microsoft.AspNetCore.Mvc;
using StarFederation.Datastar.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datastar.Controllers
{
    public class HomeController : Controller
    {
        private IServiceProvider _services;
        private IDatastarService _ds;

        public HomeController(IServiceProvider services, IDatastarService ds)
        {
            _services = services;
            _ds = ds;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MultiStepForm()
        {
            return View();
        }

        [HttpGet("displayTime")]
        public IActionResult DisplayTimeFragment()
        {
            var model = new TimeViewModel
            {
                Time = DateTime.Now.ToString("HH:mm:ss")
            };

            return PartialView(viewName: "_TimeFragment", model);
        }

        [HttpGet("displayDate")]
        public async Task DisplayDate()
        {
            string today = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
            
            // Push the fragment back as SSE
            await _ds.PatchElementsAsync($"""
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
            await _ds.RemoveElementAsync("#date");
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
            
            Signals signals = await _ds.ReadSignalsAsync<Signals>();
            Signals newSignals = new() { Output = $"{signals.Input}" };
            await _ds.PatchSignalsAsync(newSignals);
        }
    }
}
