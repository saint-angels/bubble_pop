using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BubbleType : System.IEquatable<BubbleType>
{
    public Color bubbleColor;
    public uint bubbleNumber;

    public bool Equals(BubbleType other)
    {
        return bubbleNumber == other.bubbleNumber;
    }

    public override int GetHashCode()
    {
        var hashCode = -1124373284;
        hashCode = hashCode * -1521134295 + EqualityComparer<Color>.Default.GetHashCode(bubbleColor);
        hashCode = hashCode * -1521134295 + bubbleNumber.GetHashCode();
        return hashCode;
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
