using System.Collections.Generic;
using UnityEngine;

namespace TownOfUs
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this List<T> list)
        {
            if (list.Count is 1 or 0)
                return;

            var count = list.Count;

            for (var i = 0; i <= count - 1; ++i)
            {
                var r = Random.Range(i, count);
                (list[r], list[i]) = (list[i], list[r]);
            }
        }

        public static T TakeFirst<T>(this List<T> list)
        {
            list.Shuffle();
            var item = list[0];
            list.RemoveAt(0);
            list.Shuffle();
            return item;
        }

        public static T Ability<T>(this List<T> list)
        {
            var item = list[0];
            return item;
        }
    }
}