using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringHelper
{
    public static string MoneyFormat(int number, string currency)
    {
        if (currency.ToLower() == "idr")
        {
            return number.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
        }
        else
        {
            return number.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
    public static string MoneyFormat(float number, string currency)
    {
        if (currency.ToLower() == "idr")
        {
            return number.ToString("#,0.##", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
        }
        else
        {
            return number.ToString("#,0.##", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
    public static string MoneyFormat(decimal number, string currency)
    {
        if (currency.ToLower() == "idr")
        {
            return number.ToString("#,0.##", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
        }
        else
        {
            return number.ToString("#,0.##", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public static string MoneyFormat(string input, string currency)
    {
        int decimalIndex = input.IndexOf('.');
        if (decimalIndex != -1)
        {
            input = input.Substring(0, decimalIndex);
        }

        if (long.TryParse(input, out long number))
        {
            if (currency.ToLower() == "idr")
            {
                return number.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
            }
            else
            {
                return number.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        else
        {
            return "0";
        }
    }

}
