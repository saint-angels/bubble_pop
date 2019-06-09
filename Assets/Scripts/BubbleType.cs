using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public struct BubbleType : System.IEquatable<BubbleType>
{
    [UnityEngine.Serialization.FormerlySerializedAs("bubbleColor")]
    public Color color;
    [UnityEngine.Serialization.FormerlySerializedAs("bubbleNumber")]
    public uint number;

    private static readonly SortedDictionary<int, string> abbrevations = new SortedDictionary<int, string>
     {
         {1000,"K"},
         {1000000, "M" },
         {1000000000, "B" }
     };

    public string GetHudString()
    {
        for (int i = abbrevations.Count - 1; i >= 0; i--)
        {
            KeyValuePair<int, string> pair = abbrevations.ElementAt(i);
            if (Mathf.Abs(number) >= pair.Key)
            {
                int roundedNumber = Mathf.FloorToInt(number / pair.Key);
                return roundedNumber.ToString() + pair.Value;
            }
        }
        return number.ToString();
    }

    public bool Equals(BubbleType other)
    {
        return number == other.number;
    }

    public override int GetHashCode()
    {
        return new { color, number }.GetHashCode();
    }

    public static bool operator ==(BubbleType type1, BubbleType type2)
    {
        return type1.Equals(type2);
    }

    public static bool operator !=(BubbleType type1, BubbleType type2)
    {
        return type1.Equals(type2) == false;
    }
}
