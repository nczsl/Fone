// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Util.Security {

    public static class SecurityNetCore {
        public static string EncryptAes(this string plainText, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7) {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            byte[] encrypted;
            using (var aesAlg = Aes.Create()) {
                aesAlg.Key = SecurityConfig.config.AesKey;
                aesAlg.IV = SecurityConfig.config.AesIv;
                aesAlg.Mode = mode;
                aesAlg.Padding = padding;
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (var msEncrypt = new MemoryStream()) {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                        using (var swEncrypt = new StreamWriter(csEncrypt)) {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }
        public static string DecryptAes(this string cipherTextstr, CipherMode mode = CipherMode.CBC) {
            var cipherText = Convert.FromBase64String(cipherTextstr);
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            string plaintext = null;
            using (var aesAlg = Aes.Create()) {
                aesAlg.Key = SecurityConfig.config.AesKey;
                aesAlg.IV = SecurityConfig.config.AesIv;
                aesAlg.Mode = mode;
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (var msDecrypt = new MemoryStream(cipherText)) {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                        using (var srDecrypt = new StreamReader(csDecrypt)) {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
        //-------------------------
        /// <summary>
        /// DES + Base64 加密
        /// </summary>
        /// <param name="input">明文字符串</param>
        /// <returns>已加密字符串</returns>
        public static string EncryptDes(this string input, CipherMode mode = CipherMode.CBC) {
            var des = System.Security.Cryptography.DES.Create();
            des.Mode = mode;
            byte[] key, iv;
            key = iv = default(byte[]);
            switch (mode) {
                case CipherMode.CBC:
                    key = SecurityConfig.config.DesKeyCBC; iv = SecurityConfig.config.DesIvCBC;
                    break;
                case CipherMode.ECB:
                    key = SecurityConfig.config.DesKeyECB; iv = SecurityConfig.config.DesIvECB;
                    break;
                case CipherMode.CFB:
                case CipherMode.OFB:
                case CipherMode.CTS:
                    throw new Exception("此实现不支持输出反馈模式(OFB,CTS,CFB)");
            }
            var ct = des.CreateEncryptor(key, iv);
            var byt = Encoding.Default.GetBytes(input);
            var answer = default(byte[]);
            using (var ms = new MemoryStream()) {
                using (var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write)) {
                    cs.Write(byt, 0, byt.Length);
                    cs.FlushFinalBlock();
                }
                answer = ms.ToArray();
            }
            return Convert.ToBase64String(answer);
        }
        /// <summary>
        /// DES + Base64 解密
        /// </summary>
        /// <param name="input">密文字符串</param>
        /// <returns>解密字符串</returns>
        public static string DecryptDes(this string input, CipherMode mode = CipherMode.CBC) {
            var des = System.Security.Cryptography.DES.Create();
            des.Mode = mode;
            byte[] key, iv;
            key = iv = default(byte[]);
            switch (mode) {
                case CipherMode.CBC:
                    key = SecurityConfig.config.DesKeyCBC; iv = SecurityConfig.config.DesIvCBC;
                    break;
                case CipherMode.ECB:
                    key = SecurityConfig.config.DesKeyECB; iv = SecurityConfig.config.DesIvECB;
                    break;
                case CipherMode.CFB:
                case CipherMode.OFB:
                case CipherMode.CTS:
                    throw new Exception("此实现不支持输出反馈模式(OFB,CTS,CFB)");
            }
            var ct = des.CreateDecryptor(key, iv);
            var byt = Convert.FromBase64String(input);
            var answer = default(byte[]);
            using (var ms = new MemoryStream()) {
                using (var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write)) {
                    cs.Write(byt, 0, byt.Length);
                    cs.FlushFinalBlock();
                }
                answer = ms.ToArray();
            }
            return Encoding.Default.GetString(answer);
        }
        /// <summary>
        /// 3DES 加密 Byte[] to HEX string
        /// </summary>
        /// <param name="input">明文字符串</param>
        /// <returns>已加密字符串</returns>
        public static string EncryptDes3(this string input, CipherMode mode = CipherMode.CBC, Encoding e = null) {
            if (e == null) e = Encoding.UTF8;
            var result = "";
            var des = TripleDES.Create();
            des.Mode = mode;
            var ct = des.CreateEncryptor(SecurityConfig.config.Des3Key, SecurityConfig.config.Des3Iv);
            var byt = e.GetBytes(input);
            var answer = default(byte[]);
            using (var ms = new MemoryStream()) {
                using (var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write)) {
                    cs.Write(byt, 0, byt.Length);
                    cs.FlushFinalBlock();
                }
                answer = ms.ToArray();
            }
            for (var j = 0; j < answer.Length; j++) {
                result += answer[j].ToString("x").PadLeft(2, '0');
            }
            return result;
        }
        /// <summary>
        /// 3DES + HEX to byte[] 解密
        /// </summary>
        /// <param name="input">密文字符串</param>
        /// <returns>解密字符串</returns>
        public static string DecryptDes3(this string input, CipherMode mode = CipherMode.CBC, Encoding e = null) {
            if (e == null) e = Encoding.UTF8;
            var des = System.Security.Cryptography.TripleDES.Create();
            des.Mode = mode;
            var ct = des.CreateDecryptor(SecurityConfig.config.Des3Key, SecurityConfig.config.Des3Iv);
            if (input.Length <= 1) {
                throw new Exception("encrypted HEX string is too short!");
            }
            var byt = new byte[input.Length / 2];
            for (var i = 0; i < byt.Length; i++) {
                byt[i] = Convert.ToByte(input.Substring(i * 2, 2), 16);
            }
            var answer = default(byte[]);
            using (var ms = new MemoryStream()) {
                using (var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write)) {
                    cs.Write(byt, 0, byt.Length);
                    cs.FlushFinalBlock();
                }
                answer = ms.ToArray();
            }
            return e.GetString(answer);
        }
        //--
        /// <summary>
        /// DSA+SHA1,默认私钥 签名
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SignDsa(this string data, string dsaParam = null, Encoding e = null) {
            //var dsa = DSA.Create();
            //var dsap = default(DSAParameters);
            //if (dsaParam == null) dsap = JsonConvert.DeserializeObject<DSAParameters>(Util.SecurityConfig.config.DsaAll);
            //dsa.ImportParameters(dsap);
            //var databytes = e.GetBytes(data);
            //var result = dsa.CreateSignature(databytes);
            //return Convert.ToBase64String(result, Base64FormattingOptions.None);
            //信息摘要,sha1
            if (e == null) e = Encoding.Default;
            var _sha1 = new SHA1CryptoServiceProvider();
            //var _encoding = Encoding.Default;
            var bb = e.GetBytes(data);
            var bMessageDigest = _sha1.ComputeHash(bb);
            var _DSA = new DSACryptoServiceProvider();
            //通过私钥进行签名
            //--------------------------------------------
            //var _privateKey = _DSA.ExportParameters(true);
            var dsap = System.Text.Json.JsonSerializer.Deserialize<DSAParameters>(SecurityConfig.config.DsaPfx);
            _DSA.ImportParameters(dsap);
            var DSAFormatter = new DSASignatureFormatter(_DSA);
            DSAFormatter.SetHashAlgorithm("sha1");
            var bSignature = DSAFormatter.CreateSignature(bMessageDigest);
            return Convert.ToBase64String(bSignature);
        }
        /// <summary>
        /// DSA+SHA1 默认公钥验签
        /// </summary>
        /// <param name="sign"></param>
        /// <param name="originalData"></param>
        /// <returns></returns>
        public static bool VerifyDsa(this string sign, string originalData, string dsaParam = null, Encoding e = null) {
            //信息摘要,sha1
            var _sha1 = new SHA1CryptoServiceProvider();
            //var _encoding = Encoding.Default;
            if (e == null) e = Encoding.Default;
            var bb = e.GetBytes(originalData);
            var bMessageDigest = _sha1.ComputeHash(bb);
            var _DSA = new DSACryptoServiceProvider();
            var _publicKey = System.Text.Json.JsonSerializer.Deserialize<DSAParameters>(SecurityConfig.config.DsaCer);
            _DSA.ImportParameters(_publicKey);

            var DSADeformatter = new DSASignatureDeformatter(_DSA);
            DSADeformatter.SetHashAlgorithm("sha1");
            //var bSignature = e.GetBytes(sign);
            var bSignature = Convert.FromBase64String(sign);
            var bCheck = DSADeformatter.VerifySignature(bMessageDigest, bSignature);
            return bCheck;
        }

        // Computes a keyed hash for a source file and creates a target file with the keyed hash
        // prepended to the contents of the source file. 
        /// <summary>
        /// 根据key值对源文件生成一个签名文件
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destFile"></param>
        public static void SignFileHMACSHA256(byte[] key, string sourceFile, string destFile) {
            // Initialize the keyed hash object.
            using (var hmac = new HMACSHA256(key)) {
                using (var inStream = new FileStream(sourceFile, FileMode.Open)) {
                    using (var outStream = new FileStream(destFile, FileMode.Create)) {
                        // Compute the hash of the input file.
                        var hashValue = hmac.ComputeHash(inStream);
                        // Reset inStream to the beginning of the file.
                        inStream.Position = 0;
                        // Write the computed hash value to the output file.
                        outStream.Write(hashValue, 0, hashValue.Length);
                        // Copy the contents of the sourceFile to the destFile.
                        int bytesRead;
                        // read 1K at a time
                        var buffer = new byte[1024];
                        do {
                            // Read from the wrapping CryptoStream.
                            bytesRead = inStream.Read(buffer, 0, 1024);
                            outStream.Write(buffer, 0, bytesRead);
                        } while (bytesRead > 0);
                    }
                }
            }
        } // end SignFile
        // Compares the key in the source file with a new key created for the data portion of the file. If the keys 
        // compare the data has not been tampered with.
        /// <summary>
        /// 验签源文件,查看传Key 和签名文件是否一至
        /// </summary>
        /// <param name="signedFile"></param>
        /// <returns></returns>
        public static bool VerifyFileHMACSHA256(byte[] key, string signedFile) {
            var err = false;
            // Initialize the keyed hash object. 
            using (var hmac = new HMACSHA256(key)) {
                // Create an array to hold the keyed hash value read from the file.
                var storedHash = new byte[hmac.HashSize / 8];
                // Create a FileStream for the source file.
                using (var inStream = new FileStream(signedFile, FileMode.Open)) {
                    // Read in the storedHash.
                    inStream.Read(storedHash, 0, storedHash.Length);
                    // Compute the hash of the remaining contents of the file.
                    // The stream is properly positioned at the beginning of the content, 
                    // immediately after the stored hash value.
                    var computedHash = hmac.ComputeHash(inStream);
                    // compare the computed hash with the stored value
                    for (var i = 0; i < storedHash.Length; i++) {
                        if (computedHash[i] != storedHash[i]) {
                            err = true;
                        }
                    }
                }
            }
            if (err) {
                return false;
            } else {
                return true;
            }
        } //end VerifyFile
          //---
        public static string SignSha256(this string source, Encoding e = null) {
            if (e == null) e = Encoding.UTF8;
            var sha256 = SHA256.Create();
            sha256.Initialize();
            var pwdbytes = sha256.ComputeHash(e.GetBytes(source));
            //return e.GetString(pwdbytes);
            return BitConverter.ToString(pwdbytes).Replace("-", "");
        }
        public static string SignSha384(this string source, Encoding e = null) {
            if (e == null) e = Encoding.UTF8;
            var sha384 = SHA384.Create();
            sha384.Initialize();
            var pwdbytes = sha384.ComputeHash(e.GetBytes(source));
            //return e.GetString(pwdbytes); 
            return BitConverter.ToString(pwdbytes).Replace("-", "");
        }
        public static string SignSha512(this string source, Encoding e = null) {
            if (e == null) e = Encoding.UTF8;
            var sha512 = SHA512.Create();
            sha512.Initialize();
            var pwdbytes = sha512.ComputeHash(e.GetBytes(source));
            // return e.GetString(pwdbytes);
            return BitConverter.ToString(pwdbytes).Replace("-", "");
        }
        /// <summary>
        /// .net 452 mscorlib.dll
        /// </summary>
        /// <param name="source"></param>
        /// <param name="encoding"></param>
        /// <param name="x">"x2"为32位,"x3"为48位,"x4"为64位 大写对应有效</param>
        /// <returns></returns>
        public static string SignMd5x(this string source, Encoding e = null, string x = "x2") {
            if (e == null) e = Encoding.UTF8;
            var sor = e.GetBytes(source);
            var md5 = MD5.Create();
            var result = md5.ComputeHash(sor);
            var strbul = new StringBuilder(40);
            foreach (var item in result) {
                strbul.Append(item.ToString(x));
            }
            return strbul.ToString();
        }
        /// <summary>
        /// .net 452 mscorlib.dll
        /// </summary>
        /// <param name="source"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string SignMd5(this string source, Encoding e = null) {
            if (e == null) e = Encoding.UTF8;
            var sor = e.GetBytes(source);
            var md5 = MD5.Create();
            var result = md5.ComputeHash(sor);
            var strbul = BitConverter.ToString(result).Replace("-", "");
            return strbul.ToString();
        }
        public static string SignMd5Base64(this string source, Encoding e = null) {
            if (e == null) e = Encoding.UTF8;
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(e.GetBytes(source));
            return Convert.ToBase64String(hash);
        }
        public static bool VerifyMd5(this string md5, string original, Encoding e = null, string x = "x2") {
            var _md5 = MD5.Create();
            var hashOfInput = SignMd5x(original, e, x);
            var comparer = StringComparer.OrdinalIgnoreCase;
            if (0 == comparer.Compare(hashOfInput, md5)) {
                return true;
            } else {
                return false;
            }
        }
        //---
        /// <summary>
        /// rsa解密
        /// </summary>
        /// <param name="source">加密后字符串字符串</param>
        /// <param name="xmlAllKey">加密key,包含public 和private 的xml串，一般为pfx文件</param>
        /// <returns></returns>
        public static string DecryptRsa(this string source, string xmlAllKey = null, Encoding e = null) {
            if (e == null) e = Encoding.UTF8;
            if (string.IsNullOrEmpty(source)) throw new ArgumentException("An empty string value cannot be encrypted.");
            //var rsa = RSA.Create();
            var rsa = new RSACryptoServiceProvider();
            if (string.IsNullOrWhiteSpace(xmlAllKey)) {
                xmlAllKey = SecurityConfig.config.RSAPfx;
            }
            rsa.FromXmlStringExtensions(xmlAllKey);
            //var cipherbytes = rsa.DecryptValue(Convert.FromBase64String(source));
            var cipherbytes = rsa.Decrypt(Convert.FromBase64String(source), false);
            return e.GetString(cipherbytes);
        }
        /// <summary>
        /// rsa加密
        /// </summary>
        /// <param name="source">要加密的字符串</param>
        /// <param name="xmlPublicKey">加密key 公钥</param>
        /// <returns></returns>
        public static string EncryptRsa(this string source, string xmlPublicKey = null, Encoding e = null) {
            //if(string.IsNullOrWhiteSpace(xmlPublicKey))xmlPublicKey=SecurityConfig.config.x
            if (e == null) e = Encoding.UTF8;
            if (string.IsNullOrEmpty(source)) throw new ArgumentException("An empty string value cannot be encrypted.");
            //var rsa = RSA.Create();
            var rsa = new RSACryptoServiceProvider();
            if (string.IsNullOrWhiteSpace(xmlPublicKey)) {
                xmlPublicKey = SecurityConfig.config.RSACer;
            }
            rsa.FromXmlStringExtensions(xmlPublicKey);
            var cipherbytes = rsa.Encrypt(e.GetBytes(source), false);
            return Convert.ToBase64String(cipherbytes);
        }
        public static string EncryptRSA(this string source, string xmlPublicKey = null, Encoding e = null) {
            if (e == null) e = Encoding.UTF8;
            if (string.IsNullOrWhiteSpace(xmlPublicKey)) {
                xmlPublicKey = SecurityConfig.config.RSACer;
            }
            using (var rsa = new RSACryptoServiceProvider()) {
                var PlaintextData = e.GetBytes(source);
                rsa.FromXmlStringExtensions(xmlPublicKey);
                var MaxBlockSize = rsa.KeySize / 8 - 11;    //加密块最大长度限制
                if (PlaintextData.Length <= MaxBlockSize)
                    return Convert.ToBase64String(rsa.Encrypt(PlaintextData, false));
                using (var PlaiStream = new MemoryStream(PlaintextData))
                using (var CrypStream = new MemoryStream()) {
                    var Buffer = new byte[MaxBlockSize];
                    var BlockSize = PlaiStream.Read(Buffer, 0, MaxBlockSize);
                    while (BlockSize > 0) {
                        var ToEncrypt = new byte[BlockSize];
                        Array.Copy(Buffer, 0, ToEncrypt, 0, BlockSize);
                        var Cryptograph = rsa.Encrypt(ToEncrypt, false);
                        CrypStream.Write(Cryptograph, 0, Cryptograph.Length);
                        BlockSize = PlaiStream.Read(Buffer, 0, MaxBlockSize);
                    }
                    return Convert.ToBase64String(CrypStream.ToArray(), Base64FormattingOptions.None);
                }
            }
        }
        public static string DecryptRSA(this string source, string xmlPrivateKey = null, Encoding e = null) {
            if (e == null) e = Encoding.UTF8;
            if (string.IsNullOrEmpty(xmlPrivateKey)) {
                xmlPrivateKey = SecurityConfig.config.RSAPfx;
            }
            using (var rsa = new RSACryptoServiceProvider()) {
                var CiphertextData = Convert.FromBase64String(source);
                rsa.FromXmlStringExtensions(xmlPrivateKey);
                var MaxBlockSize = rsa.KeySize / 8;    //解密块最大长度限制
                if (CiphertextData.Length <= MaxBlockSize)
                    return e.GetString(rsa.Decrypt(CiphertextData, false));
                using (var CrypStream = new MemoryStream(CiphertextData))
                using (var PlaiStream = new MemoryStream()) {
                    var Buffer = new byte[MaxBlockSize];
                    var BlockSize = CrypStream.Read(Buffer, 0, MaxBlockSize);
                    while (BlockSize > 0) {
                        var ToDecrypt = new byte[BlockSize];
                        Array.Copy(Buffer, 0, ToDecrypt, 0, BlockSize);
                        var Plaintext = rsa.Decrypt(ToDecrypt, false);
                        PlaiStream.Write(Plaintext, 0, Plaintext.Length);
                        BlockSize = CrypStream.Read(Buffer, 0, MaxBlockSize);
                    }
                    return e.GetString(PlaiStream.ToArray());
                }
            }
        }
        /// <summary>
        /// 扩展FromXmlString
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="xmlString"></param>
        public static void FromXmlStringExtensions(this RSA rsa, string xmlString) {
            var parameters = new RSAParameters();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue")) {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes) {
                    switch (node.Name) {
                        case "Modulus": parameters.Modulus = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "Exponent": parameters.Exponent = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "P": parameters.P = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "Q": parameters.Q = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "DP": parameters.DP = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "DQ": parameters.DQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "InverseQ": parameters.InverseQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "D": parameters.D = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                    }
                }
            } else {
                throw new Exception("Invalid XML RSA key.");
            }
            rsa.ImportParameters(parameters);
        }
        /// <summary>
        /// 扩展ToXmlString
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="includePrivateParameters"></param>
        /// <returns></returns>
        public static string ToXmlStringExtensions(this RSA rsa, bool includePrivateParameters) {
            var parameters = rsa.ExportParameters(includePrivateParameters);
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                  parameters.Modulus != null ? Convert.ToBase64String(parameters.Modulus) : null,
                  parameters.Exponent != null ? Convert.ToBase64String(parameters.Exponent) : null,
                  parameters.P != null ? Convert.ToBase64String(parameters.P) : null,
                  parameters.Q != null ? Convert.ToBase64String(parameters.Q) : null,
                  parameters.DP != null ? Convert.ToBase64String(parameters.DP) : null,
                  parameters.DQ != null ? Convert.ToBase64String(parameters.DQ) : null,
                  parameters.InverseQ != null ? Convert.ToBase64String(parameters.InverseQ) : null,
                  parameters.D != null ? Convert.ToBase64String(parameters.D) : null);
        }
    }

    public static class SecurityConfig {
        internal static SecurityConfigContent config;
        static SecurityConfig() {
            config = System.Text.Json.JsonSerializer.Deserialize<SecurityConfigContent>(Properties.Resources.SecurityConfig);
        }
        /// <summary>
        /// 返回重置所有密钥后的文本，
        /// </summary>
        /// <returns></returns>
        public static string ResetConfig() {
            var rsa = RSA.Create();
            var includePrivateParameters = true;
            var parameters = rsa.ExportParameters(includePrivateParameters);
            if (includePrivateParameters) {
                config.RSAPfx = string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                    Convert.ToBase64String(parameters.Modulus),
                    Convert.ToBase64String(parameters.Exponent),
                    Convert.ToBase64String(parameters.P),
                    Convert.ToBase64String(parameters.Q),
                    Convert.ToBase64String(parameters.DP),
                    Convert.ToBase64String(parameters.DQ),
                    Convert.ToBase64String(parameters.InverseQ),
                    Convert.ToBase64String(parameters.D));
            }
            config.RSACer = string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                    Convert.ToBase64String(parameters.Modulus),
                    Convert.ToBase64String(parameters.Exponent));
            var des = DES.Create();
            des.Mode = CipherMode.CBC;
            des.GenerateIV();
            des.GenerateKey();
            config.DesIvCBC = des.IV;
            config.DesKeyCBC = des.Key;
            des.Mode = CipherMode.ECB;
            des.GenerateIV();
            des.GenerateKey();
            config.DesIvECB = des.IV;
            config.DesKeyECB = des.Key;
            var aes = new AesCryptoServiceProvider();
            aes.GenerateIV();
            aes.GenerateKey();
            config.AesIv = aes.IV;
            config.AesKey = aes.Key;
            var dsa = new DSACryptoServiceProvider();
            var dsapp = dsa.ExportParameters(true);
            var dsap = default(Dsap);
            dsap.Set(dsapp);
            config.DsaPfx = System.Text.Json.JsonSerializer.Serialize(dsap);
            var dsapc = dsa.ExportParameters(false);
            config.DsaCer = System.Text.Json.JsonSerializer.Serialize(dsapc);
            var dest = new TripleDESCryptoServiceProvider();
            dest.GenerateIV();
            dest.GenerateKey();
            config.TripleDESIv = dest.IV;
            config.TripleDESKey = dest.Key;
            var des3 = TripleDES.Create();
            des3.GenerateIV();
            des3.GenerateKey();
            config.Des3Iv = des3.IV;
            config.Des3Key = des3.Key;
            var setting = new System.Text.Json.JsonSerializerOptions();

            var json = System.Text.Json.JsonSerializer.Serialize(config);
            return json;
        }
    }

    public class SecurityConfigContent {
        public byte[] DesIvCBC { get; set; }
        public byte[] DesKeyCBC { get; set; }
        public byte[] DesIvECB { get; set; }
        public byte[] DesKeyECB { get; set; }
        public byte[] Des3Key { get; set; }
        public byte[] Des3Iv { get; set; }
        public byte[] AesIv { get; set; }
        public byte[] AesKey { get; set; }
        public byte[] TripleDESIv { get; set; }
        public byte[] TripleDESKey { get; set; }
        public string RSAPfx { get; set; }
        public string RSACer { get; set; }
        public string DsaPfx { get; set; }
        public string DsaCer { get; set; }
    }
    /*因为有罕见的bug DSAParameters 序列化有问题所以做一个转接类*/
    struct Dsap {
        public int Counter;
        public byte[] Seed;
        public byte[] P;
        public byte[] G;
        public byte[] Q;
        public byte[] Y;
        public byte[] J;
        public byte[] X;
        public void Set(DSAParameters dsa) {
            this.Counter = dsa.Counter;
            this.Seed = dsa.Seed;
            this.P = dsa.P;
            this.G = dsa.G;
            this.Q = dsa.Q;
            this.Y = dsa.Y;
            this.J = dsa.J;
            this.X = dsa.X;
        }
    }
}
