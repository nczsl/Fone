using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Fone {
    public class AuthHelp {
        /// <summary>
        /// 用于在satrtup 里做配置时用
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        static public TokenValidationParameters GetJwtTokenVaidateParam(IConfiguration config) {
            var tokenParameters = new TokenValidationParameters {
                ValidateIssuer = bool.Parse(config["jwt:ValidateIssuer"]),//是否验证Issuer
                ValidateAudience = bool.Parse(config["jwt:ValidateAudience"]),//是否验证Audience
                ValidateLifetime = bool.Parse(config["jwt:ValidateLifetime"]),//是否验证失效时间
                ValidateIssuerSigningKey = bool.Parse(config["jwt:ValidateIssuerSigningKey"]),//是否验证SecurityKey
                ValidAudience = config["jwt:ValidAudience"],//Audience
                ValidIssuer = config["jwt:ValidIssuer"],//Issuer，这两项和前面签发jwt的设置一致
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["security:key"]))//拿到SecurityKey
            };
            return tokenParameters;
        }
        //选中下面注释直接交互执行看结果
        /* #r "C:\Users\hjjls\.nuget\packages\newtonsoft.json\12.0.2\lib\netstandard2.0\Newtonsoft.Json.dll"
        static public string GetJwtConfigTokenVParam() {
           var jwt = new {
               ValidateIssuer=false,
               ValidateAudience = false,
               ValidateLifetime = false,
               ValidateIssuerSigningKey = false,
               ValidAudience = "localhost",
               ValidIssuer = "localhost",
               expires=30
           };
            var str=Newtonsoft.Json.JsonConvert.SerializeObject(jwt);
            return $"\"jwt\":{str}";
        }
        var result=GetJwtConfigTokenVParam();
        result 
        ----------------------------------or 直接copy下面------------------------------------------
          "jwt": {
            "ValidateIssuer": false,
            "ValidateAudience": false,
            "ValidateLifetime": false,
            "ValidateIssuerSigningKey": false,
            "ValidAudience": "localhost",
            "ValidIssuer": "localhost",
            "expires":30//过期时间 分钟
          },
          "security": {
            "key": "109134wpoepoweiu134134^ls."
          }
        */
        /// <summary>
        /// 构造jwtbearer token，在controller里使用
        /// </summary>
        /// <param name="tokenPayload">claim字典用于生成token payload 注意不要包含敏感信息</param>
        /// <returns></returns>
        static public string CreateJwtBearerToken(Dictionary<string, string> tokenPayload, IConfiguration config) {
            //这里就是声明我们的claim
            var claimlist = new List<Claim>();
            foreach (var item in tokenPayload) {
                claimlist.Add(new Claim(item.Key, item.Value));
            }
            //使用对称加密
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["security:key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            /*var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecurityKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);*/
            var token = new JwtSecurityToken(
                issuer: config["jwt:Issuer"],
                audience: config["jwt:Audience"],
                claims: claimlist,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(config["jwt:expires"])),//DateTime.Parse(config["jwt:expires"]),
                signingCredentials: creds);
            var Token = new JwtSecurityTokenHandler().WriteToken(token);
            return Token;
        }
        /// <summary>
        /// cookie安全验证方式 本方法不页面跳转处理,调用成功写入token到cookie中，以后客户端请求会自带上
        /// AddAuthentication(...)
        /// .AddCookie(...) 时本方法有效
        /// </summary>
        /// <param name="userUniqueInfo">之一：唯一用户名，邮箱，手机，身份证号码，等可供唯一识别用户的信息</param>
        /// <param name="ctx">HttpContext</param>
        /// <param name="schema">验证架构字符串标识</param>
        /// <returns></returns>
        static public async Task CookieLogin(string userUniqueInfo,  HttpContext ctx, string schema= "Cookies") {
            var ci = new ClaimsIdentity("idcard");
            ci.AddClaim(new Claim(ClaimTypes.NameIdentifier, userUniqueInfo));
            var cp = new ClaimsPrincipal();
            cp.AddIdentity(ci);
            await ctx.SignInAsync(schema,cp);
        }
    }
}
