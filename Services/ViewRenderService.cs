using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Datastar.Services
{
    public class ViewRenderService : IViewRenderService
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ViewRenderService(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> RenderToStringAsync(string viewName, object model)
        {
            var httpCtx = _httpContextAccessor.HttpContext
                            ?? throw new InvalidOperationException("No HttpContext");
            var actionCtx = new ActionContext(
                httpCtx,
                httpCtx.GetRouteData(),
                new ActionDescriptor()
            );

            using var sw = new StringWriter();

            var viewResult = _viewEngine.FindView(actionCtx, viewName, isMainPage: false);
            if (!viewResult.Success)
                throw new InvalidOperationException($"View \"{viewName}\" not found.");

            var viewData = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model
            };
            var tempData = new TempDataDictionary(httpCtx, _tempDataProvider);

            var viewCtx = new ViewContext(
                actionCtx,
                viewResult.View,
                viewData,
                tempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewCtx);
            return sw.ToString();
        }
    }
}
