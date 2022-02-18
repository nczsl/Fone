using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;
using Util.Security;

namespace OutInAmount.Test {
    [TestClass]
    public class Security2 {
        [TestMethod]
        public void m33() {
            var test = "252adsfq243526sdfg!$@#%!$3asdfop12r309adsfhqeaerf500";
            var aescbc=test.EncryptAes(CipherMode.CBC);
            var unaescbc=aescbc.DecryptAes(CipherMode.CBC);
            Console.WriteLine(aescbc);
            Console.WriteLine(unaescbc);
            var aesecb = test.EncryptAes(CipherMode.ECB);
            var unaesecb = aesecb.DecryptAes(CipherMode.ECB);
            Console.WriteLine(aesecb);
            Console.WriteLine(unaesecb);
            //
            var descbc = test.EncryptDes(CipherMode.CBC);
            var undescbc = descbc.DecryptDes(CipherMode.CBC);
            Console.WriteLine(descbc);
            Console.WriteLine(undescbc);
            //var desecb = test.EncryptDes(CipherMode.ECB);
            //var undesecb = descbc.DecryptDes(CipherMode.ECB);
            //Console.WriteLine(desecb);
            //Console.WriteLine(undesecb);
            //
            var des3cbc=test.EncryptDes3(CipherMode.CBC);
            var undes3cbc=des3cbc.DecryptDes3(CipherMode.CBC);
            Console.WriteLine(des3cbc);
            Console.WriteLine(undes3cbc);
            //var des3ecb = test.EncryptDes3(CipherMode.ECB);
            //var undes3ecb = des3cbc.DecryptDes3(CipherMode.ECB);
            //Console.WriteLine(des3ecb);
            //Console.WriteLine(undes3ecb);
            var rsa = test.EncryptRSA();
            var unrsa = rsa.DecryptRSA();
            Console.WriteLine(rsa);
            Console.WriteLine(unrsa);
            var rsa2 = test.EncryptRsa();
            var unrsa2 = rsa2.DecryptRsa();
            Console.WriteLine(rsa2);
            Console.WriteLine(unrsa2);
            ////
            var signdsa = test.SignDsa();
            Console.WriteLine(signdsa);
            Console.WriteLine(signdsa.VerifyDsa(test));
            var md5 = test.SignMd5();
            Console.WriteLine(md5);
            Console.WriteLine(md5.VerifyMd5(test));
            var md5base64 = test.SignMd5Base64();
            Console.WriteLine(md5base64);
            var md5x = test.SignMd5x();
            Console.WriteLine(md5x);
            var signsha256 = test.SignSha256();
            Console.WriteLine(signsha256);
            var signsha3 = test.SignSha384();
            Console.WriteLine(signsha3);
            var signsha512 = test.SignSha512();
            Console.WriteLine(signsha512);

        }
        [TestMethod]
        public void m12() {
            //Console.WriteLine(string.Join(",", SecurityConfig.config.AesIv));
            var x=SecurityConfig.ResetConfig();
            Console.WriteLine(x);
            var x2=JsonConvert.DeserializeObject<SecurityConfigContent>(x);
            Console.WriteLine(x2);
        }
    }
}
