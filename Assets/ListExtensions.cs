﻿using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions {
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts) {
        int count = ts.Count;
        int last = count - 1;
        for (int i = 0; i < last; ++i) {
            int r = Random.Range(i, count);
            (ts[i], ts[r]) = (ts[r], ts[i]);
        }
    }
}