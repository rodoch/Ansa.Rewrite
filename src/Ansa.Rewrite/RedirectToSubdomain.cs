using System;
using System.Net;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;

namespace Ansa.Rewrite
{
    /// <summary>
    /// Rewrite rule that redirects the current request to a host with a specified subdomain
    /// </summary>
    public class RedirectToSubdomain : IRule
    {
        private readonly string _subdomain;
        private readonly int _statusCode;
        private readonly bool _excludeLocalhost;

        /// <summary>
        /// Redirects the current request to a host with a specified subdomain
        /// </summary>
        /// <param name="subdomain">Specify a subdomain that will form part of the destination host</param>
        /// <param name="statusCode">Optionally specify a HTTP status code to returned with the redirect request. The default value is 301 (Moved Permanently).</param>
        /// <param name="excludeLocalhost">Exclude requests to a localhost domain from the redirect rule. The default value is true.</param>
        public RedirectToSubdomain(
            string subdomain,
            int statusCode = (int)HttpStatusCode.MovedPermanently,
            bool excludeLocalhost = true)
        {
            _subdomain = subdomain;
            _statusCode = statusCode;
            _excludeLocalhost = excludeLocalhost;
        }

        /// <summary>
        /// Applies the URL rewrite rule
        /// </summary>
        /// <param name="context">The <see cref="RewriteContext"/> object</param>
        public void ApplyRule(RewriteContext context)
        {
            var request = context.HttpContext.Request;
            var host = request.Host;

            if (host.Host.StartsWith(_subdomain, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = RuleResult.ContinueRules;
                return;
            }

            if (_excludeLocalhost && string.Equals(host.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = RuleResult.ContinueRules;
                return;
            }

            string newPath = request.Scheme + "://" + _subdomain + "." + host.Value + request.PathBase + request.Path + request.QueryString;

            var response = context.HttpContext.Response;
            response.StatusCode = _statusCode;
            response.Headers[HeaderNames.Location] = newPath;
            context.Result = RuleResult.EndResponse;
        }
    }
}