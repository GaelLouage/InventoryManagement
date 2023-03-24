using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infrastructuur.Extensions
{
    public static class Phone
    {
        public static bool IsValidPhoneNumber(this string phoneNumber)
        {
            // Phone number should only contain numbers and be 10 digits long
            var regex = new Regex(@"^\d{10}$");
            return regex.IsMatch(phoneNumber);
        }
    }
}
