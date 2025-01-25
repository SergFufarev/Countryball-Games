using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using FunnyBlox;
using FunnyBlox.PlaneReward;

[Serializable]
public class Aircraft
{
    private AircraftService _parentService;
    private Action _onDespawn;
    private Action _onExplode;

    public GameObject _gameObject;
    private Vector3 _moveDirection;
    private bool _isInteracted;

    private AircraftRoot Root => _parentService.GetAircraftRoot;

    private const float SpawnYPos = 1.0f;

    public void Initialize(AircraftService service, Action onDespawn, Action onExplode)
    {
        _parentService = service;
        _onDespawn += onDespawn;
        _onExplode += onExplode;

        _isInteracted = false;
        _gameObject.SetActive(false);
    }

    public void Spawn()
    {
        var spawnPosition = ComputeSpawnPosition();
        var moveDirection = _moveDirection;

        Root.transform.position = spawnPosition;
        Root.transform.LookAt(spawnPosition - moveDirection);

        _isInteracted = false;
        _gameObject.SetActive(true);
    }

    public IEnumerator Update()
    {
        while (true)
        {
            MoveAircraft();

            if (CanDespawn() && !_isInteracted)
            {
                _gameObject.SetActive(false);
                _onDespawn?.Invoke();

                var tutor = _parentService.GUI.TutorContainer;
                if (tutor.InTutorial && tutor.CurrentTutorialID == TutorContainer.AircraftTutorID) tutor.BreakTutorial();

                yield break;
            }

            if (CanExplode())
            {
                Explosion();
                yield break;
            }

            yield return null;
        }
    }

    private void Explosion()
    {
        _gameObject.SetActive(false);
        _onExplode?.Invoke();

        if (CommonData.VibrationsOn) Handheld.Vibrate();
    }

    public bool TryDrop(float angle)
    {
        if (_isInteracted) return false;
        _isInteracted = true;

        var direction = Quaternion.AngleAxis(angle, _parentService.GetAircraftRoot.transform.right) * _parentService.GetAircraftRoot.transform.forward;
        _parentService.GetAircraftRoot.transform.DOLookAt(_parentService.GetAircraftRoot.transform.position + direction, 0.2f);

        return true;
    }

    private void MoveAircraft()
    {
        Root.transform.position += Root.transform.forward * (Time.deltaTime * _parentService.GetAircraftSpeed);
    }

    private bool CanDespawn()
    {
        var planarAlignment = _parentService.GetAlignment();
        planarAlignment.y = 0.0f;
        var planarPosition = Root.transform.position;
        planarPosition.y = 0.0f;

        // При отдалении на критическое расстояние можем деспаунить
        return Vector3.Distance(planarAlignment, planarPosition) >= _parentService.GetDespawnRadius;
    }

    private float ExplosionY = -1;

    private bool CanExplode() =>

        Root.transform.position.y <= ExplosionY;

    private Vector3 ComputeSpawnPosition()
    {
        _moveDirection = Quaternion.Euler(0, UnityEngine.Random.Range(0, 361), 0) * Vector3.forward;
        var spawnPosition = _parentService.GetAlignment() + _moveDirection * _parentService.GetSpawnRadius;

        spawnPosition.y = SpawnYPos; // _parentService.GetSpawnHeight;

        return spawnPosition;
    }
}