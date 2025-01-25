using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FunnyBlox;

public class CountryFlagMaker : MonoBehaviour
{
    [SerializeField] private Material baseMat;
    [SerializeField] private List<Texture2D> flagSprites;
    [SerializeField] private CountriesData countriesConfig;

    //private const string Path = "Assets/_Project/Sources/Materials/CountryBall/Generated";
    private const string Path = "Assets/_Project/Sources/Materials/Country/Generated";

#if UNITY_EDITOR

    [ContextMenu("CreateFlags")]
    private void CreateFlags()
    {
        for (int i = 0; i < flagSprites.Count; i++)
        {
            string fileName = $"{i}_{flagSprites[i].name}.mat";

            Material mat = new Material(baseMat);
            mat.mainTexture = flagSprites[i];

            AssetDatabase.CreateAsset(mat, $"{Path}/{fileName}");
        }
    }

    [ContextMenu("CreateMaterials")]
    private void CreateMaterials()
    {
        for (int i = 0; i < countriesConfig.CountryData.Count; i++)
        {
            string fileName = $"{i}_Country.mat";

            CountryData data = countriesConfig.CountryData[i];
            Material mat = new Material(baseMat);

            mat.color = HexToColor(data.Color);

            AssetDatabase.CreateAsset(mat, $"{Path}/{fileName}");
        }
    }

    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }

#endif

    /*
    [ContextMenu("UpdateFlagNumbers")]
    private void UpdateFlagNumbers()
    {
        for (int i = 0; i < flagMaterials.Count; i++)
        {
            var mat = flagMaterials[i];
            mat.
        }
    }
    */
}