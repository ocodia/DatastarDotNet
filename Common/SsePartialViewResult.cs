using Datastar.Services;
using Microsoft.AspNetCore.Mvc;
using StarFederation.Datastar.DependencyInjection;

namespace Datastar.Common
{
    public class SsePartialViewResult : ActionResult
    {
        private readonly string _viewName;
        private readonly object _model;
       

        public SsePartialViewResult(string viewName, object model)
        {
            _viewName = viewName;
            _model = model;           
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var renderer = context.HttpContext.RequestServices
                             .GetRequiredService<IViewRenderService>();

            var sse = context.HttpContext.RequestServices
                             .GetRequiredService<IDatastarServerSentEventService>();

            var html = await renderer.RenderToStringAsync(_viewName, _model);
            await sse.MergeFragmentsAsync(html);
        }
    }
}
