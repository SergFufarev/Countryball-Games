using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheSTAR.Utility
{
    public static class PoolUtility
    {
        public delegate T CreatePoolObjectDelegate<T>(Vector3 pos);

        public static T GetPoolObject<T>(List<T> pool, Predicate<T> match, Vector3 pos, CreatePoolObjectDelegate<T> create) where T : MonoBehaviour
        {
            if (pool.Count == 0) return create(pos);
            else
            {
                var o = pool.Find(match);

                if (o == null) return create(pos);
                else
                {
                    o.transform.position = pos;
                    o.gameObject.SetActive(true);
                    return o;
                }
            }
        }
    }
}