using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Fone {
    /// <summary>
    /// 实现grpc客户端配置
    /// </summary>
    public interface IGrpcChannel {
        /// <summary>
        /// grpc channel字典
        /// </summary>
        Dictionary<string, Grpc.Net.Client.GrpcChannel> ChannelDic { get; }
    }
    /// <summary>
    /// 存放grpc channel 字典
    /// ...
    /// "GrpcChannels":[{"Key":"xxx","Value":"xxx"},...]
    /// ...
    /// </summary>
    public class GrpcChannelDic : IGrpcChannel {        
        /// <summary>
        /// 构造函数，若可选注入字典为空时，使用appsettings.json中的一个
        /// ‘GrpcChannels 的节点下面由一个数组的键值对构成[{"Key":"xxx","Value",xxx}...]
        /// </summary>
        /// <param name="config">默认指向appsettings.json</param>
        /// <param name="addressDic">可选注入字典</param>
        public GrpcChannelDic(IConfiguration config, Dictionary<string, string> addressDic=null) {
            ChannelDic = new Dictionary<string, Grpc.Net.Client.GrpcChannel>();
            if (addressDic!=null) {
                foreach (var item in addressDic) {
                    ChannelDic.Add(item.Key, Grpc.Net.Client.GrpcChannel.ForAddress(item.Value));
                }
            } else {
                foreach (var item in config.GetSection("GrpcChannels").GetChildren()) {
                    ChannelDic.Add(item["Key"], Grpc.Net.Client.GrpcChannel.ForAddress(item["Value"]));
                }
            }
        }
        /// <summary>grpc channel 字典</summary>
        public Dictionary<string, Grpc.Net.Client.GrpcChannel> ChannelDic {get; }            
    }
    
}
