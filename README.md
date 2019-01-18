# Ansa.Rewrite

[![NuGet Pre Release](https://img.shields.io/nuget/vpre/Ansa.Rewrite.svg)](https://www.nuget.org/packages/Ansa.Rewrite/)
[![NuGet](https://img.shields.io/nuget/dt/Ansa.Rewrite.svg)](https://www.nuget.org/packages/Ansa.Rewrite/)

This is a small library designed to cater for common URL rewriting and redirection tasks in ASP.NET Core web applications. It provides you with a collection of rules that you can apply as part of your URL [rewriting middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/url-rewriting?view=aspnetcore-2.2). I will be adding more rules to the library as I discover use cases for them.

## Install and setup

Add the NuGet package [Ansa.Rewrite](https://www.nuget.org/packages/Ansa.Rewrite/) to any ASP.NET Core 2.1+ project.

```cmd
dotnet add package Ansa.Rewrite
```

Add `using Ansa.Rewrite` to your **Startup.cs** file and apply the rewrite rules within the *Configure* method:

```C#
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsProduction())
    {
        var subdomain = new RewriteOptions()
            .Add(new RedirectToSubdomain("www"));
        app.UseRewriter(subdomain);
    }
}
```

It is also possible to chain rewrite rules:

```C#
var domainRedirects = new RewriteOptions()
    .Add(new RedirectTopLevelDomain(new[] { ".ie", ".eu" }, ".com"))
    .Add(new RedirectToSubdomain("www"));
```

## Rules

This section lists the rules currently made available by Ansa.Rewrite.

### RedirectToSubdomain

Redirects the current request to a host with a specified subdomain. For example, your website is accessible at both `example.com` and `www.example.com` and you want to make `www.example.com` the canonical domain:

```C#
var subdomain = new RewriteOptions()
    .Add(new RedirectToSubdomain("www"));
app.UseRewriter(subdomain);
```

### RedirectTopLevelDomain

Redirects the current request to a host with a different top-level domain (TLD). Use this rule when you own multiple TLDs (e.g. .com, .net, .eu) and you wish to redirect requests to these domains to one canonical TLD:

```C#
var domainRedirects = new RewriteOptions()
    .Add(new RedirectTopLevelDomain(new[] { ".ie", ".eu" }, ".com", "www"))
```

The source TLDs are given passed in as a string array in the first argument. The target or canonical domain is given as the second argument. Optionally, you can specify a target subdomain as a fourth argument. This is the same as chaining a **RedirectToSubdomain** rule to the TLD request except that the rewrite is preformed in one hop and the client receives one redirect response instead of two. This is more desirable in terms of SEO.

## Coming soon

- RedirectToNoSubdomain (for directs such as `https://www.example.com` to `https://example.com`)
- RedirectToHost (redirect to a completely different URL, e.g. www.example.com to www.sample.com)