using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FunnyBlox.Editor
{

    public class ItemMenu
    {
        [MenuItem("Tools/Wipe PlayerPrefs")]
        static void PlayerPrefsDeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

    }
}