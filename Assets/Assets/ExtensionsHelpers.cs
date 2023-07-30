using System.Collections.Generic;
using UnityEngine;

internal static class ExtensionsHelpers
{
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            int k = random.Next(n--);
            T temp = list[n];
            list[n] = list[k];
            list[k] = temp;
        }
    }

    public static Color32 HexToColor(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        else
        {
            Debug.LogError("Invalid Hex String: " + hex);
            return new Color32(0, 0, 0, 255);  // default return value (black) if invalid hex string
        }
    }
}