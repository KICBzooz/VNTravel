using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VNTravel.Models
{
    public class RandomID
    {
        public static string rdID_Voucher(int i = 5)
        {
            const string chars = "0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, i).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string rdID_Customer(int i = 5)
        {
            const string chars = "0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, i).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}