using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;
using Battle;
using TheSTAR.Utility;

namespace FunnyBlox
{
    [CreateAssetMenu(fileName = "CountriesData", menuName = "Data/Countries Data", order = 51)]
    public class CountriesData : ScriptableObject
    {
        [Searchable]
        public List<CountryData> CountryData;
        public List<CountryCollider> CountryPrefabs;
        public List<Material> FlagMaterials;

        public Material _unexploredTerritoryMaterial;

        [Space]
        public Material _minMaterialUnexplored;
        public Material _minMaterialPlayer;
        public Material _minMaterialEnemy;

        public Material[] _countryMaterialsFull;
        public Material[] _customPlayerMaterials;

        [Header("Buildings")]
        [SerializeField] private CountryBuildingVisual[] buildingVisuals;
        [SerializeField] private UnityDictionary<WonderType, WonderConfigData> wonderDatas;

        [Space]
        public UnityDictionary<CountryForceType, IntRange> countryForceData = new();

        public CountryBuildingVisual GetBuildingVisual(int index) => buildingVisuals[index];
        public CountryBuildingVisual GetWonderVisual(WonderType wonderType) => wonderDatas.Get(wonderType).VisualPrefab;
        public Sprite GetWonderIcon(WonderType wonderType) => wonderDatas.Get(wonderType).Icon;

        [Serializable]
        public class WonderConfigData
        {
            [SerializeField] private CountryBuildingVisual visualPrefab;
            [SerializeField] private Sprite icon;

            public CountryBuildingVisual VisualPrefab => visualPrefab;
            public Sprite Icon => icon;
        }

#if UNITY_EDITOR
        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        //[ContextMenu("Sort")]
        private void Sort()
        {
            CountryData.Sort();
        }

        //[ContextMenu("UpdateIDs")]
        private void UpdateIDs()
        {
            for (int i = 0; i < CountryData.Count; i++)
            {
                CountryData[i].Id = i;
            }
        }

        //[ContextMenu("SetDefaultBallPositions")]
        private void SetDefaultBallPositions()
        {
            foreach (var countryData in CountryData)
            {
                countryData.GenerateDefaultBallPositions();
            }
        }

        //[ContextMenu("SetBallPosYtoZ")]
        private void SetBallPosYtoZ()
        {
            foreach (var countryData in CountryData)
            {
                for (int i = 0; i < countryData.countryBallDatas.ballPositions.Length; i++)
                {
                    countryData.countryBallDatas.ballPositions[i].smartPosZ = countryData.countryBallDatas.ballPositions[i].smartPosY;
                }
            }
        }

        //[ContextMenu("SetDefaultYPos")]
        private void SetDefaultYPos()
        {
            foreach (var countryData in CountryData)
            {
                for (int i = 0; i < countryData.countryBallDatas.ballPositions.Length; i++)
                {
                    countryData.countryBallDatas.ballPositions[i].smartPosY = CountryBallVisual.DefaultPosY;
                }
            }
        }

        //[ContextMenu("RandomizeWonders")]
        private void RandomizeWonders()
        {
            foreach (var countryData in CountryData)
            {
                countryData.SetWonder((WonderType)(UnityEngine.Random.Range(0, 6)));
            }

            Save();

            //Debug.Log("Wonder randomized");
        }

        [ContextMenu("RandomizeCustoms")]
        private void RandomizeCustoms()
        {
            foreach (var countryData in CountryData)
            {
                countryData.SetRandomCustomisation();
            }

            Save();

            Debug.Log("Customs randomized");
        }

        [ContextMenu("SetDefaultArmyData")]
        private void SetDefaultArmyData()
        {
            Debug.Log("Set Default Army Data...");

            foreach (var countryData in CountryData)
            {
                countryData.SetDefaultArmyData();
            }

            Save();

            Debug.Log("Complete");
        }

        //[ContextMenu("ParseGD")]
        private void ParseGD()
        {
            for (int i = 0; i < CountryData.Count; i++)
            {
                //if (gdConfig.CountryData[i].hatType != CustomizationHatType.None) CountryData[i].hatType = gdConfig.CountryData[i].hatType;
                //if (gdConfig.CountryData[i].faceType != CustomizationFaceType.Basic) CountryData[i].faceType = gdConfig.CountryData[i].faceType;
            }

            Save();

            Debug.Log("Parse complete");
        }
#endif
    }

    public enum WonderType
    {
        Default,
        Default_1,
        Default_2,
        Default_3,
        Default_4,
        Default_5,
        Big_Ben,
        CN_Tower,
        Eiffel_Tower,
        Gyeongbokkun,
        Kiomizu,
        Kremlin,
        Mausoleum_in_Halicarnassus,
        PagamaCanal,
        PetronasTower,
        RiceTerraces,
        RuhrIndComp,
        Statue_of_Liberty
    }
}