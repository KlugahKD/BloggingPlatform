using System.Text.RegularExpressions;

namespace BloggingPlatform.Business.Helpers;

public static class PhoneNumber
{
    public static string? CorrectPhoneNumber(string number)
    {
        const string countryCode = "233";
        string cleanedNumber = Regex.Replace(number, @"\D", "");

        if (cleanedNumber.Length < 9)
        {
            return null;
        }

        if (cleanedNumber.StartsWith(countryCode) && cleanedNumber.Length == 12)
        {
            return cleanedNumber;
        }

        if (cleanedNumber.StartsWith('0') && cleanedNumber.Length == 10)
        {
            return countryCode + cleanedNumber.Substring(1);
        }

        if (cleanedNumber.Length == 9)
        {
            return countryCode + cleanedNumber;
        }
        
        return null;
    }
}