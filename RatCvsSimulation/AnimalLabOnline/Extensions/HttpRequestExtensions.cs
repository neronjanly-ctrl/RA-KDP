using Microsoft.AspNetCore.Http;

namespace AnimalLabOnline
{
    public static class HttpRequestExtensions
    {
        public static bool IsAjax(this HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        public static bool IsAjaxRequest(this HttpContext context)
        {
            return context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}
