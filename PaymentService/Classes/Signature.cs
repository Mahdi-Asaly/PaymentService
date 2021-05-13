using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaymentService.Classes
{
    public class Signature
    {
        public static string GenerateSHA256(string value)
        {
            System.Security.Cryptography.SHA256 sh = System.Security.Cryptography.SHA256.Create();
            byte[] hashValue = sh.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value));
            return System.Convert.ToBase64String(hashValue);
        }
    }
}