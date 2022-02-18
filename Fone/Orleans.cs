using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Config;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
//
namespace Fone.Orleans.Client {
    public class ClusterClientHostedService : IHostedService {
        private readonly ILogger<ClusterClientHostedService> _logger;
        public ClusterClientHostedService(ILogger<ClusterClientHostedService> logger, IClusterClient client) {
            _logger = logger;
            Client = client;
        }
        public Task StartAsync(CancellationToken cancellationToken) {
            var attempt = 0;
            var maxAttempts = 100;
            var delay = TimeSpan.FromSeconds(1);
            return Client.Connect(async error => {
                if (cancellationToken.IsCancellationRequested) {
                    return false;
                }
                if (++attempt < maxAttempts) {
                    _logger.LogWarning(error,
                        "Failed to connect to Orleans cluster on attempt {@Attempt} of {@MaxAttempts}.",
                        attempt, maxAttempts);
                    try {
                        await Task.Delay(delay, cancellationToken);
                    } catch (OperationCanceledException) {
                        return false;
                    }
                    return true;
                } else {
                    _logger.LogError(error,
                        "Failed to connect to Orleans cluster on attempt {@Attempt} of {@MaxAttempts}.",
                        attempt, maxAttempts);
                    return false;
                }
            });
        }
        public async Task StopAsync(CancellationToken cancellationToken) {
            try {
                await Client.Close();
            } catch (OrleansException error) {
                _logger.LogWarning(error, "Error while gracefully disconnecting from Orleans cluster. Will ignore and continue to shutdown.");
            }
        }
        public IClusterClient Client { get; }
    }
}
namespace Fone.Orleans.Server {
    static public partial class Ex {
        /// <summary>
        /// 需要依赖正确的appsettings.json
        /// 格式参考：
        /// https://github.com/dotnet/orleans/blob/master/Samples/OneBoxDeployment/src/OneBoxDeployment.OrleansHost/appsettings.orleanshost.Development.json
        /// </summary>
        /// <param name="isb">ISiloBuilder</param>
        /// <param name="configuration">配置文档</param>
        /// <param name="assemblys">grain 实现集</param>
        /// <param name="isCluster">是否本地测试配置</param>
        /// <returns>ISiloBuilder</returns>
        static public ClusterConfig SiloSettings(this ISiloBuilder isb, IConfiguration configuration, bool isCluster = false) {
            var clusterConfig = configuration.GetSection(nameof(ClusterConfig)).Get<ClusterConfig>();
            var jso = new JsonSerializerOptions();
            jso.Converters.Add(new IPAddressConverter());
            jso.Converters.Add(new AssemblyConverter());
            jso.Converters.Add(new IPEndPointConverter());
            string getCurrentJson(string value) => $"\"{value}\"";
            clusterConfig.EndPointOptions.AdvertisedIPAddress = JsonSerializer.Deserialize<IPAddress>(getCurrentJson(configuration[$"{nameof(ClusterConfig)}:{nameof(ClusterConfig.EndPointOptions)}:{nameof(ClusterConfig.EndPointOptions.AdvertisedIPAddress)}"]), jso);
            clusterConfig.EndPointOptions.SiloListeningEndpoint = JsonSerializer.Deserialize<IPEndPoint>(getCurrentJson(configuration[$"{nameof(ClusterConfig)}:{nameof(ClusterConfig.EndPointOptions)}:{nameof(ClusterConfig.EndPointOptions.SiloListeningEndpoint)}"]), jso);
            clusterConfig.EndPointOptions.GatewayListeningEndpoint = JsonSerializer.Deserialize<IPEndPoint>(getCurrentJson(configuration[$"{nameof(ClusterConfig)}:{nameof(ClusterConfig.EndPointOptions)}:{nameof(ClusterConfig.EndPointOptions.GatewayListeningEndpoint)}"]), jso);
            var assemblyPartsValue = configuration.GetSection($"{nameof(ClusterConfig)}:{nameof(ClusterConfig.AssemblyParts)}").Get<string[]>();
            foreach (var item in assemblyPartsValue) {
                clusterConfig.AssemblyParts.Add(JsonSerializer.Deserialize<Assembly>(getCurrentJson(item), jso));
            }
            if (!isCluster) {
                isb.UseLocalhostClustering(
                    gatewayPort: clusterConfig.EndPointOptions.GatewayPort,
                    clusterId: clusterConfig.ClusterOptions.ClusterId,
                    serviceId: clusterConfig.ClusterOptions.ServiceId
                    );
                isb.Configure<EndpointOptions>(options => {
                    options.AdvertisedIPAddress = clusterConfig.EndPointOptions.AdvertisedIPAddress ?? IPAddress.Loopback;
                });
            } else {
                //ado 数据库连接和提供程序在host端配置
                isb.Configure<ClusterOptions>(options => {
                    options.ClusterId = clusterConfig.ClusterOptions.ClusterId;
                    options.ServiceId = clusterConfig.ClusterOptions.ServiceId;
                });
                isb.ConfigureEndpoints(
                    siloPort:clusterConfig.EndPointOptions.SiloPort, 
                    gatewayPort:clusterConfig.EndPointOptions.GatewayPort,
                    advertisedIP:clusterConfig.EndPointOptions.AdvertisedIPAddress,
                    listenOnAnyHostAddress:true
                    );
                //isb.Configure<EndpointOptions>(options => {
                //    options.SiloPort = clusterConfig.EndPointOptions.SiloPort;
                //    options.GatewayPort = clusterConfig.EndPointOptions.GatewayPort;
                //    options.AdvertisedIPAddress = clusterConfig.EndPointOptions.AdvertisedIPAddress ?? IPAddress.Loopback;
                //    options.GatewayListeningEndpoint = clusterConfig.EndPointOptions.GatewayListeningEndpoint ?? new IPEndPoint(IPAddress.Any, options.GatewayPort);
                //    options.SiloListeningEndpoint = clusterConfig.EndPointOptions.SiloListeningEndpoint ?? new IPEndPoint(IPAddress.Any, options.SiloPort);                    
                //});
            }
            foreach (var item in clusterConfig.AssemblyParts) {
                isb.ConfigureApplicationParts(parts => parts.AddApplicationPart(item).WithReferences());
            }
            return clusterConfig;
        }
    }
}
namespace Orleans.Config {
    /// <summary>
    /// Orleans cluster configuration parameters.
    /// </summary>
    //[DebuggerDisplay("ClusterConfig(ServiceId = {ClusterOptions.ServiceId}, ClusterId = {ClusterOptions.ClusterId})")]    
    public sealed class ClusterConfig {
        /// <summary>
        /// The cluster configuration information.
        /// </summary>
        public ClusterOptions ClusterOptions { get; set; } = new ClusterOptions();
        /// <summary>
        /// The cluster endpoint information.
        /// </summary>   
        public EndpointOptions EndPointOptions { get; set; } = new EndpointOptions();
        /// <summary>
        /// Configuration to membership storage.
        /// </summary>
        public ConnectionConfig ConnectionConfig { get; set; } = new ConnectionConfig();
        /// <summary>
        /// The persistent storage configurations.
        /// </summary>
        public IList<ConnectionConfig> StorageConfigs { get; set; } = new List<ConnectionConfig>();
        /// <summary>
        /// The persistent reminder configurations.
        /// </summary>
        public IList<ConnectionConfig> ReminderConfigs { get; set; } = new List<ConnectionConfig>();
        /// <summary>
        /// The AssemblyPart configurations.
        /// </summary>
        public IList<Assembly> AssemblyParts { get; set; } = new List<Assembly>();
    }
    // [DebuggerDisplay("ConnectionConfig(Name = {Name}, ConnectionString = {ConnectionString}, AdoNetConstant = {AdoNetConstant})")]
    public sealed class ConnectionConfig {
        /// <summary>
        /// The name of the grain storage configuration.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The connection string to the storage.
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// This is optional and applies only to ADO.NET.
        /// </summary>
        public string AdoNetConstant { get; set; }
    }
    public class AssemblyConverter : JsonConverter<Assembly> {
        public override Assembly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var item = reader.GetString();
            return Assembly.Load(item);
        }
        public override void Write(Utf8JsonWriter writer, Assembly value, JsonSerializerOptions options) {
            writer.WriteStringValue(value.FullName);
        }
    }
    public class IPAddressConverter : JsonConverter<IPAddress> {
        public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var v = reader.GetString();
            return IPAddress.Parse(v);
        }

        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options) {
            writer.WriteStringValue(value.ToString());
        }
    }
    public class IPEndPointConverter : JsonConverter<IPEndPoint> {
        public override IPEndPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var x = reader.GetString();
            if (string.IsNullOrWhiteSpace(x)) return null;
            var v = x.Split(":");
            var address = IPAddress.Parse(v[0]);
            var ipep = new IPEndPoint(address, 0);
            if (v.Length == 2) {
                ipep.Port = int.Parse(v[1]);
            }
            return ipep;
        }

        public override void Write(Utf8JsonWriter writer, IPEndPoint value, JsonSerializerOptions options) {
            writer.WriteStringValue($"{value.Address}:{value.Port}");
        }
    }
}