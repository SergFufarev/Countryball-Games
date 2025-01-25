using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;

/// <summary>
/// Разбивает большую карту на объекты стран
/// Как использовать:
/// 1) Закинуть на сцену объект, содержащий все страны (глобальную карту)
/// 2) Прокинуть все ссылки для CountryMaker
/// 3) Нажать "GenerateCountries", в итоге все страны разделятся на нужные объекты
/// 4) Объекты можно запрефабить и использовать в настройках
/// </summary>
public class CountryMaker : MonoBehaviour
{
    [SerializeField] private CountryCollider emptyCountryPrefab;
    [SerializeField] private List<GameObject> visualParts;
    [SerializeField] private Transform resultContainer;
    [SerializeField] private bool useMeshCollider = false;

    [ContextMenu("GenerateCountries")]
    private void GenerateCountries()
    {
        CountryCollider currentCountry;
        GameObject currentVisualPart;
        for (int i = 0; i < visualParts.Count; i++)
        {
            currentVisualPart = visualParts[i];
            currentCountry = Instantiate(emptyCountryPrefab, currentVisualPart.transform.position, Quaternion.identity, resultContainer);
            currentVisualPart.transform.parent = currentCountry.transform;
            currentCountry.name = $"{i}_{currentVisualPart.name}";

            if (useMeshCollider) currentCountry.CreateMeshCollider();
        }
    }
}