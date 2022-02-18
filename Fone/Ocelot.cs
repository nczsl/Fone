using System.Collections.Generic;

namespace Fone.Ocelot {

    public class OcelotSetting {
        public Reroute[] ReRoutes { get; set; }
        public GlobalConfiguration GlobalConfiguration { get; set; }
    }

    public class Reroute {
        public string DownstreamPathTemplate { get; set; }
        public string UpstreamPathTemplate { get; set; }
        public string[] UpstreamHttpMethod { get; set; }
        public Dictionary<string, string> AddHeadersToRequest { get; set; }
        public Dictionary<string, string> UpstreamHeaderTransform { get; set; }
        public Dictionary<string, string> DownstreamHeaderTransform { get; set; }
        public Dictionary<string, string> AddClaimsToRequest { get; set; }
        public Dictionary<string, string> RouteClaimsRequirement { get; set; }
        public Dictionary<string, string> AddQueriesToRequest { get; set; }
        public object RequestIdKey { get; set; }
        public Filecacheoptions FileCacheOptions { get; set; }
        public bool ReRouteIsCaseSensitive { get; set; }
        public object ServiceName { get; set; }
        public string DownstreamScheme { get; set; }
        public Qosoptions QoSOptions { get; set; }
        public Loadbalanceroptions LoadBalancerOptions { get; set; }
        public Ratelimitoptions RateLimitOptions { get; set; }
        public Authenticationoptions AuthenticationOptions { get; set; }
        public Httphandleroptions HttpHandlerOptions { get; set; }
        public Downstreamhostandport[] DownstreamHostAndPorts { get; set; }
        public object UpstreamHost { get; set; }
        public object Key { get; set; }
        public object[] DelegatingHandlers { get; set; }
        public int Priority { get; set; }
        public int Timeout { get; set; }
        public bool DangerousAcceptAnyServerCertificateValidator { get; set; }
    }

    public class Filecacheoptions {
        public int TtlSeconds { get; set; }
        public object Region { get; set; }
    }

    public class Qosoptions {
        public int ExceptionsAllowedBeforeBreaking { get; set; }
        public int DurationOfBreak { get; set; }
        public int TimeoutValue { get; set; }
    }

    public class Loadbalanceroptions {
        public string Type { get; set; }
        public object Key { get; set; }
        public int Expiry { get; set; }
    }

    public class Ratelimitoptions {
        public object[] ClientWhitelist { get; set; }
        public bool EnableRateLimiting { get; set; }
        public object Period { get; set; }
        public float PeriodTimespan { get; set; }
        public int Limit { get; set; }
    }

    public class Authenticationoptions {
        public object AuthenticationProviderKey { get; set; }
        public object[] AllowedScopes { get; set; }
    }

    public class Httphandleroptions {
        public bool AllowAutoRedirect { get; set; }
        public bool UseCookieContainer { get; set; }
        public bool UseTracing { get; set; }
        public bool UseProxy { get; set; }
    }

    public class Downstreamhostandport {
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public class GlobalConfiguration {
        public Servicediscoveryprovider Servicediscoveryprovider { get; set; }
    }
    public class Servicediscoveryprovider {
        public string Host { get; set; }
        public int Port { get; set; }
        public string ConfigurationKey { get; set; }
    }

}
