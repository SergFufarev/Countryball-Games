using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using TheSTAR.Utility;
using Battle;

namespace FunnyBlox
{
    // todo для будущих версий прийти к разделению CountryInGameData и CountryConfigData
    [Serializable]
    public class CountryData : IComparable<CountryData>
    {
        // for config
        public string Name;
        [FoldoutGroup("ReadOnly")] [ReadOnly] public int Id;
        [FoldoutGroup("ReadOnly")] [ReadOnly] public string Flag;
        [FoldoutGroup("ReadOnly")] [ReadOnly] public int Owner;

        // for game
        [FoldoutGroup("ReadOnly")] [ReadOnly] public bool IsBaseCountry;
        [FoldoutGroup("ReadOnly")] [ReadOnly] public bool OpenState;
        [Obsolete][FoldoutGroup("ReadOnly")] [ReadOnly] public bool isWonderBuilded; // для наката, не используется в версии с Big Battle

        [JsonIgnore]
        public bool UseAirArmy => CheckUseBall(CountryBallType.AirArmy);

        [JsonIgnore]
        public bool UseNavalArmy => CheckUseBall(CountryBallType.NavalArmy);

        public bool CheckUseBall(CountryBallType countryBallType)
        {
            return countryBallType switch
            {
                CountryBallType.Main => true,
                CountryBallType.GroundArmy => true,
                CountryBallType.AirArmy => Factories >= 3,
                CountryBallType.NavalArmy => Factories >= 2,
                CountryBallType.Resistance => !IsBaseCountry,
                CountryBallType.Factory => true,
                CountryBallType.Intelligence => IsBaseCountry,
                _ => false,
            };
        }

        // for config
        [JsonIgnore]
        public float ScaleFactor;
        public float MaxSizeValue; // до какого максимального скейла может увеличиваться главный кантрибол страны при отдалении камеры

        [JsonIgnore]
        public string Color;

        // for config
        public int Factories; // количество производства
        public int BaseForce; // базовая сила территории

        [JsonIgnore]
        public bool IsPlayerOwner => Owner == CommonData.PlayerID;

        // for config
        [Space]
        [JsonIgnore]
        public CountryBallPositionDatas countryBallDatas;

        // for config
        [Space]
        [JsonIgnore]
        public bool useBuildings = true;

        // for config
        [JsonIgnore]
        [SerializeField] [ShowIf("useBuildings")] private WonderType wonderType;

        [JsonIgnore]
        public WonderType WonderType => wonderType;

        public void SetWonder(WonderType wonder)
        {
            wonderType = wonder;
        }

        [JsonIgnore]
        [ShowIf("useBuildings")]
        public BuildingPositionDatas buildingsDatas;

        [Header("Customisation")]
        [JsonIgnore]
        public CustomizationHatType hatType;
        [JsonIgnore]
        public CustomizationFaceType faceType;

        [Header("Army")]
        [JsonIgnore]
        public CountryArmyData armyData;

        public void CloneBallPositions(CountryBallPositionDatas from)
        {
            countryBallDatas = new();
            countryBallDatas.ballPositions = new CountryBallPositionData[from.ballPositions.Length];

            for (int i = 0; i < from.ballPositions.Length; i++)
            {
                countryBallDatas.ballPositions[i] = from.ballPositions[i];
            }
        }

        public void CloneBuildingPositions(BuildingPositionDatas from)
        {
            buildingsDatas = new();
            buildingsDatas.positions = new BuildingPositionData[from.positions.Length];

            for (int i = 0; i < from.positions.Length; i++)
            {
                buildingsDatas.positions[i] = from.positions[i];
            }

            buildingsDatas.wonderPosition = from.wonderPosition;
        }

        public Vector3? PositionForBall(CountryBallType ballType)
        {
            if (ballType == CountryBallType.Intelligence) ballType = CountryBallType.Resistance;

            if (countryBallDatas == null) return null;

            bool dataFound = false;

            CountryBallPositionData positionData = new CountryBallPositionData();

            for (int i = 0; i < countryBallDatas.ballPositions.Length; i++)
            {
                if (countryBallDatas.ballPositions[i].ballType == ballType)
                {
                    positionData = countryBallDatas.ballPositions[i];
                    dataFound = true;
                    break;
                }
            }

            if (!dataFound) return null;

            if (!positionData.useSmartPos) return null;
            else return new Vector3(positionData.smartPosX, positionData.smartPosY, positionData.smartPosZ);
        }

        public Vector3 PositionForBuilding(int index)
        {
            Vector3 result;

            if (buildingsDatas != null && buildingsDatas.positions.Length > index)
            {
                result = new Vector3(buildingsDatas.positions[index].x, buildingsDatas.positions[index].y, buildingsDatas.positions[index].z);
            }
            else result = Vector3.zero;

            return result;
        }

        public Vector3 PositionForWonder()
        {
            Vector3 result;

            if (buildingsDatas != null)
            {
                result = new Vector3(buildingsDatas.wonderPosition.x, buildingsDatas.wonderPosition.y, buildingsDatas.wonderPosition.z);
            }
            else result = Vector3.zero;

            return result;
        }

        public float RotationForBuilding(int index)
        {
            if (buildingsDatas == null) return 0;

            if (buildingsDatas.positions.Length > index) return buildingsDatas.positions[index].rotation;
            else return 0;
        }

        public float RotationForWonder()
        {
            if (buildingsDatas == null) return 0;
            return buildingsDatas.wonderPosition.rotation;
        }

        public void SetSmartPositionForBall(CountryBallType ballType, float x, float y, float z)
        {
            for (int i = 0; i < countryBallDatas.ballPositions.Length; i++)
            {
                if (countryBallDatas.ballPositions[i].ballType == ballType)
                {
                    countryBallDatas.ballPositions[i].SetSmartPos(x, y, z);
                    return;
                }
            }

            // шарик не найден
            var newBallsArray = new CountryBallPositionData[countryBallDatas.ballPositions.Length + 1];

            for (int i = 0; i < countryBallDatas.ballPositions.Length; i++)
            {
                newBallsArray[i] = countryBallDatas.ballPositions[i];
            }

            countryBallDatas.ballPositions = newBallsArray;
            countryBallDatas.ballPositions[^1] = new CountryBallPositionData(ballType, x, y, z);
        }

        public CountryData()
        {
        }

        public CountryData(CountryData data)
        {
            Id = data.Id;
            
            Name = data.Name;
            Flag = data.Flag;
            Owner = data.Owner;
            OpenState = data.OpenState;
            IsBaseCountry = data.IsBaseCountry;

            ScaleFactor = data.ScaleFactor;

            countryBallDatas = data.countryBallDatas;
            buildingsDatas = data.buildingsDatas;
            Factories = data.Factories;

            Color = data.Color;
            MaxSizeValue = data.MaxSizeValue;
            useBuildings = data.useBuildings;
            wonderType = data.wonderType;

            faceType = data.faceType;
            hatType = data.hatType;
        }
        [Serializable]
        public class CountryBallPositionDatas
        {
            public CountryBallPositionData[] ballPositions;
        }

        [Serializable]
        public class BuildingPositionDatas
        {
            public BuildingPositionData[] positions;
            public BuildingPositionData wonderPosition;

            public void Set(int index, float x, float y, float z, float rotation)
            {
                positions[index].Set(x, y, z, rotation);
            }
        }

        [Serializable]
        public struct CountryBallPositionData
        {
            public CountryBallType ballType;

            public bool useSmartPos;
            [ShowIf("useSmartPos")] public float smartPosX;
            [ShowIf("useSmartPos")] public float smartPosY;
            [ShowIf("useSmartPos")] public float smartPosZ;

            public CountryBallPositionData(CountryBallType ballType)
            {
                this.ballType = ballType;
                useSmartPos = false;
                smartPosX = 0;
                smartPosY = 0;
                smartPosZ = 0;
            }

            public CountryBallPositionData(CountryBallType ballType, float x, float y, float z)
            {
                this.ballType = ballType;
                useSmartPos = true;
                smartPosX = x;
                smartPosY = y;
                smartPosZ = z;
            }

            public void SetSmartPos(float x, float y, float z)
            {
                smartPosX = x;
                smartPosY = y;
                smartPosZ = z;
                useSmartPos = true;
            }
        }

        [ContextMenu("SetDefaultArmyData")]
        public void SetDefaultArmyData()
        {
            armyData.SetDefault();
        }

        [Serializable]
        public class CountryArmyData
        {
            public UnityDictionary<UnitType, IntRange> armyDictionary;

            public void SetDefault()
            {
                var allUnitTypes = EnumUtility.GetValues<UnitType>();
                armyDictionary = new UnityDictionary<UnitType, IntRange>();

                foreach (var armyType in allUnitTypes)
                {
                    if (armyType == UnitType.Rebels) continue;

                    armyDictionary.Add(armyType, new IntRange(0, 3));
                }
            }

            public UnityDictionary<UnitType, int> GetResultUpgrades(int totalNeededForce)
            {
                // получаем значение по-честному (с соблюдением всех диапазонов armyDictionary)

                UnityDictionary<UnitType, int> resultArmy = new ();

                int resultForce = 0;

                var allUnitTypes = EnumUtility.GetValues<UnitType>();
                for (int i = 0; i < allUnitTypes.Length; i++)
                {
                    var unitType = allUnitTypes[i];
                    if (!armyDictionary.Contains(unitType)) continue;

                    int force = UnityEngine.Random.Range(armyDictionary.Get(unitType).min, armyDictionary.Get(unitType).max + 1);
                    resultArmy.Add(unitType, force);
                    resultForce += force;
                }

                if (resultForce == totalNeededForce) return resultArmy;

                // 2 видоизменяем не выходя за дианазоны указанные в конфиге

                if (resultForce > totalNeededForce)
                {
                    for (int i = allUnitTypes.Length - 2; i >= 0; i--)
                    {
                        var unitType = allUnitTypes[i];
                        if (!armyDictionary.Contains(unitType)) continue;

                        while (resultArmy.Get(unitType) > armyDictionary.Get(unitType).min && resultForce > totalNeededForce)
                        {
                            resultArmy.Set(unitType, resultArmy.Get(unitType) - 1);
                            resultForce--;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < allUnitTypes.Length; i++)
                    {
                        var unitType = allUnitTypes[i];
                        if (!armyDictionary.Contains(unitType)) continue;

                        while (resultArmy.Get(unitType) < armyDictionary.Get(unitType).max && resultForce < totalNeededForce)
                        {
                            resultArmy.Set(unitType, resultArmy.Get(unitType) + 1);
                            resultForce++;
                        }
                    }
                }

                if (resultForce == totalNeededForce) return resultArmy;

                
                // 3 видоизменяем выходя за дианазоны конфига, но не выходя за диапазон 0 и 3

                if (resultForce > totalNeededForce)
                {
                    for (int i = allUnitTypes.Length - 2; i >= 0; i--)
                    {
                        var unitType = allUnitTypes[i];
                        if (!armyDictionary.Contains(unitType)) continue;

                        while (resultArmy.Get(unitType) > 0 && resultForce > totalNeededForce)
                        {
                            resultArmy.Set(unitType, resultArmy.Get(unitType) - 1);
                            resultForce--;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < allUnitTypes.Length; i++)
                    {
                        var unitType = allUnitTypes[i];
                        if (!armyDictionary.Contains(unitType)) continue;

                        while (resultArmy.Get(unitType) < 3 && resultForce < totalNeededForce)
                        {
                            resultArmy.Set(unitType, resultArmy.Get(unitType) + 1);
                            resultForce++;
                        }
                    }
                }

                if (resultForce == totalNeededForce) return resultArmy;

                // 4 видоизменяем выходя за дианазоны конфига, минимальное значение 0, макимальное не ограничено

                if (resultForce > totalNeededForce)
                {
                    for (int i = allUnitTypes.Length - 2; i >= 0; i--)
                    {
                        var unitType = allUnitTypes[i];
                        if (!armyDictionary.Contains(unitType)) continue;

                        while (resultArmy.Get(unitType) > 0)
                        {
                            resultArmy.Set(unitType, resultArmy.Get(unitType) - 1);
                            resultForce--;
                            if (resultForce == totalNeededForce) break;
                        }
                    }
                }
                else
                {
                    int difference = totalNeededForce - resultForce;

                    for (int i = 0; i < allUnitTypes.Length; i++)
                    {
                        var unitType = allUnitTypes[i];
                        if (!armyDictionary.Contains(unitType)) continue;

                        resultArmy.Set(unitType, resultArmy.Get(unitType) + difference);
                        resultForce += difference;
                        break;
                    }
                }

                return resultArmy;
            }
        }

        [Serializable]
        public struct BuildingPositionData
        {
            public float x;
            public float y;
            public float z;
            public float rotation;

            public BuildingPositionData(float x, float y, float z, float rotation)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.rotation = rotation;
            }

            public void Set(float x, float y, float z, float rotation)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.rotation = rotation;
            }
        }

        public void GenerateDefaultBallPositions()
        {
            countryBallDatas.ballPositions = new CountryBallPositionData[]
            {
                new CountryBallPositionData(CountryBallType.Main, 0, CountryBallVisual.DefaultPosY, CountryBallVisual.MainBallPosZ),
                new CountryBallPositionData(CountryBallType.GroundArmy),
                new CountryBallPositionData(CountryBallType.AirArmy),
                new CountryBallPositionData(CountryBallType.NavalArmy),
                new CountryBallPositionData(CountryBallType.Resistance),
                new CountryBallPositionData(CountryBallType.Factory),
                new CountryBallPositionData(CountryBallType.Intelligence),
            };
        }

        public int CompareTo(CountryData other) => Name.CompareTo(other.Name);

#if UNITY_EDITOR
        public void SetRandomCustomisation()
        {
            hatType = EnumUtility.GetRandomValue<CustomizationHatType>();
            faceType = EnumUtility.GetRandomValue<CustomizationFaceType>();
        }
#endif
    }
}