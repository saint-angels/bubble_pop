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
