using Datastar.Services;
using Microsoft.AspNetCore.Mvc;
using StarFederation.Datastar.DependencyInjection;

namespace Datastar.Common
{
    public class SsePartialViewResult : ActionResult
    {
        private readonly string _viewName;
        private readonly object _model;
        private readonly IViewRenderService _renderer;
        private readonly IDatastarServerSentEventService _sse;

        public SsePartialViewResult(
            string viewName,
            object model,
            IViewRenderService renderer,
            IDatastarServerSentEventService sse)
        {
            _viewName = viewName;
            _model = model;
            _renderer = renderer;
            _sse = sse;
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var html = await _renderer.RenderToStringAsync(_viewName, _model);
            await _sse.MergeFragmentsAsync(html);
        }
    }
}
