using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;
using System;
using System.Net;

namespace Ansa.Rewrite
{
    /// <summary>
    /// Rewrite rule that redirects the current request to a new hostname
    /// </summary>
    public class RedirectToHostname : IRule
    {
        private readonly string[] _domains;
        private readonly string _targetHostname;
        private readonly string _subdomain;
        private readonly int _statusCode;
        private readonly bool _excludeLocalhost;

        /// <summary>
        /// Redirects the current request to a new hostname (e.g. sample.com to example.com)
        /// </summary>
        /// <param name="domains">An array of one or more domains to which the rule should be applied</param>
        /// <param name="targetHostname">The destination hostname to which requests should be redirected</param>
        /// <param name="subdomain">Optionally specify a subdomain that will form part of the destination host</param>
        /// <param name="statusCode">Optionally specify a HTTP status code to returned with the redirect request. The default value is 301 (Moved Permanently).</param>
        /// <param name="excludeLocalhost">Exclude requests to a localhost domain from the redirect rule. The default value is true.</param>
        public RedirectToHostname(
            string[] domains,
            string targetHostname,
            string subdomain = "",
            int statusCode = (int)HttpStatusCode.MovedPermanently,
            bool excludeLocalhost = true)
        {
            _domains = domains;
            _targetHostname = targetHostname;
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

            if (string.Equals(host.Value, _targetHostname, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = RuleResult.ContinueRules;
                return;
            }

            if (_excludeLocalhost && string.Equals(host.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = RuleResult.ContinueRules;
                return;
            }

            var newHost = default(string);
            
            if (!string.IsNullOrWhiteSpace(_subdomain))
            {
                newHost = _subdomain + ".";
            }

            newHost += _targetHostname;
            var newHostString = (host.Port is null) ? new HostString(newHost) : new HostString(newHost, (int)host.Port);
            var newPath = UriHelper.BuildAbsolute(request.Scheme, newHostString, request.PathBase, request.Path, request.QueryString);

            var response = context.HttpContext.Response;
            response.StatusCode = _statusCode;
            response.Headers[HeaderNames.Location] = newPath;
            context.Result = RuleResult.EndResponse;
        }
    }
}