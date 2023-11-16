using System.Collections.Generic;
using System;

/// <summary>
/// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
/// Provides a method for performing a deep copy of an object.
/// Binary Serialization is used to perform the copy.
/// </summary>
public static class ObjectUtils
{
    public static T Clone<T>(T source)
    {
        var serialized = UnityEngine.JsonUtility.ToJson(source);
        return UnityEngine.JsonUtility.FromJson<T>(serialized);
    }

    private static Random random = new Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}