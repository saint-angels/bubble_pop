using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class NumberFormatHelper
{
    private static readonly SortedDictionary<ulong, string> abbrevations = new SortedDictionary<ulong, string>
     {
         {1000,"K"},
         {1000000, "M" },
         {1000000000, "B" }
     };

    public static string FormatNumberBubble(int bubblePower)
    {
        ulong bubbleNumber = (ulong)Mathf.Pow(2, bubblePower);

        for (int i = abbrevations.Count - 1; i >= 0; i--)
        {
            KeyValuePair<ulong, string> pair = abbrevations.ElementAt(i);
            if (Mathf.Abs(bubbleNumber) >= pair.Key)
            {
                int roundedNumber = Mathf.FloorToInt(bubbleNumber / pair.Key);
                return roundedNumber.ToString() + pair.Value;
            }
        }
        return bubbleNumber.ToString();
    }

    public static string FormatNumberScore(uint number)
    {
        for (int i = abbrevations.Count - 1; i >= 0; i--)
        {
            KeyValuePair<ulong, string> pair = abbrevations.ElementAt(i);
            if (Mathf.Abs(number) >= pair.Key)
            {
                int roundedNumber = Mathf.FloorToInt(number / pair.Key);
                return roundedNumber.ToString() + pair.Value;
            }
        }
        return number.ToString();
    }
}
