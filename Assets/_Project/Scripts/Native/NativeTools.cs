using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FunnyBlox.Native
{
    public class NativeTools : MonoBehaviour
    {

        void Start()
        {
#if UNITY_IOS || UNITY_ANDROID
            Application.targetFrameRate = 60;
#endif
        }
    }
}