using System;
using System.Net;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;

namespace Ansa.Rewrite
{
    /// <summary>
    /// Rewrite rule that redirects the current request to a host with a different top-level domain (e.g. .com, .eu)
    /// </summary>
    public class RedirectTopLevelDomain : IRule
    {
        private readonly string[] _topLevelDomains;
        private readonly string _targetDomain;
        private readonly string _subdomain;
        private readonly bool _excludeLocalhost;
        private readonly int _statusCode;

        /// <summary>
        /// Redirects the current request to a host with a different top-level domain (e.g. .com, .eu)
        /// </summary>
        /// <param name="topLevelDomains">An array of one or more top-level domains to which the rule should be applied</param>
        /// <param name="targetDomain">The destination top-level domain to which requests should be redirected</param>
        /// <param name="subdomain">Optionally specify a subdomain that will form part of the destination host</param>
        /// <param name="statusCode">Optionally specify a HTTP status code to returned with the redirect request. The default value is 301 (Moved Permanently).</param>
        /// <param name="excludeLocalhost">Exclude requests to a localhost domain from the redirect rule. The default value is true.</param>
        public RedirectTopLevelDomain(
            string[] topLevelDomains,
            string targetDomain,
            string subdomain = "",
            int statusCode = (int)HttpStatusCode.MovedPermanently,
            bool excludeLocalhost = true)
        {
            _topLevelDomains = topLevelDomains;
            _targetDomain = targetDomain;
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

            if (host.Host.EndsWith(_targetDomain, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = RuleResult.ContinueRules;
                return;
            }

            if (_excludeLocalhost && string.Equals(host.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = RuleResult.ContinueRules;
                return;
            }

            var newHost = host.Host;

            foreach (var domain in _topLevelDomains)
            {
                if (host.Host.EndsWith(domain, StringComparison.OrdinalIgnoreCase))
                {
                    newHost = host.Host.Replace(domain, _targetDomain);
                    continue;
                }
            }

            if (!string.IsNullOrWhiteSpace(_subdomain)
                && !host.Host.StartsWith(_subdomain, StringComparison.OrdinalIgnoreCase))
            {
                newHost = _subdomain + "." + newHost;
            }

            var newPath = request.Scheme + "://" + newHost + request.PathBase + request.Path + request.QueryString;

            var response = context.HttpContext.Response;
            response.StatusCode = _statusCode;
            response.Headers[HeaderNames.Location] = newPath;
            context.Result = RuleResult.EndResponse;
        }
    }
}