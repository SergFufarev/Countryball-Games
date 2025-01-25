using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;
using TheSTAR.Utility;
using Sirenix.OdinInspector;

public class TutorContainer : MonoBehaviour, ISaver
{
    [SerializeField] private bool use;

    [Space]
    [ShowIf("use")] [SerializeField] private TutorCursor cursor;
    [ShowIf("use")] [SerializeField] private Camera tutorCamera;
    [ShowIf("use")] [SerializeField] private Canvas tutorialCanvas;

    [Space]
    [ShowIf("use")] [SerializeField] private Transform upPos;
    [ShowIf("use")] [SerializeField] private Transform bottomPos;
    [ShowIf("use")] [SerializeField] private Transform leftPos;
    [ShowIf("use")] [SerializeField] private Transform rightPos;

    [Space]
    [ShowIf("use")] [SerializeField] private bool showDebugs = true;

    private bool _inTutorial = false;
    private Transform _currentFocusTran;
    private TutorialBasingType? _currentTutorialBasingType;
    private bool _autoUpdatePos = false;
    private string _currentTutorialID = null;
    private List<string> _completedTutorials = null;

    private const int UiOrder = 10;
    private const int WorldOrder = 0;

    public const string FactoryTutorID = "factory";
    public const string ArmyTutorID = "army";
    public const string IntelligenceTutorID = "intelligence";
    public const string EnemyAttackTutorID = "attack_enemy";
    public const string NewFactoryTutorID = "factory_in_new_city";
    public const string ResistanceTutorID = "resistance";
    public const string TradeTutorID = "trade";
    public const string TradeCloseTutorID = "trade_close";
    public static readonly string[] ArmyInfo_TutorIDs = new string[]
    {
        "army_info_0",
        "army_info_1",
        "army_info_2",
        "army_info_3",
        "army_info_4",
        "army_info_5",
        "army_info_6",
        "army_info_7",
        "army_info_8",
        "army_info_9"
    };
    public const string RabelsTutorID = "rabels";
    public const string CountriesScreenTutorID = "countries_screen";
    public const string BuyCustomHatTutorID = "buy_custom_hat";
    public const string GetUnitInBattlePrepareTutorID = "get_unit_in_battle_prepare";
    public const string BuyRocketTutorID = "buy_rocket";
    public const string UnitPlacementTutorID = "unit_placement";
    public const string BigBattleAttackTutorID = "attack_in_big_battle";
    public const string RocketAttackTutorID = "rocket_attack";

    // new
    public const string DailyBonusTutorID = "get_first_daily";
    public const string AircraftTutorID = "aircraft";

    private readonly Dictionary<string, string> tutorStepsToAnalytics = new Dictionary<string, string>()
    {
        { ArmyTutorID, "1_first_army_upgrade" },
        { EnemyAttackTutorID, "2_first_attack_on_country" },
        { FactoryTutorID, "3_buying_economic_upgrade" }
    };

    public string CurrentTutorialID => _currentTutorialID;
    public bool InTutorial => _inTutorial;

    public bool IsComplete(string id) => _completedTutorials != null && _completedTutorials.Contains(id);

    public bool IsLastTutorialComplete => IsComplete(ResistanceTutorID);

    private readonly Dictionary<CursorViewType, CursorTransformData> cursorViewDatas = new()
    {
        { CursorViewType.Default, new(new(0, 0, 0), new(1, 1, 1)) },
        { CursorViewType.UpEdge, new(new(0, 0, 0), new(1, 1, 1)) },
        { CursorViewType.BottomEnge, new(new(0, 0, 0), new(1, -1, 1)) },
        { CursorViewType.LeftEdge, new(new(0, 0, 90), new(-1, 1, 1)) },
        { CursorViewType.RightEdge, new(new(0, 0, -90), new(1, 1, 1)) },
        { CursorViewType.Invisible, new(new(0, 0, 0), new(1, 1, 1)) }
    };

    private CursorViewType _currentCursorViewType;

    private void Awake()
    {
        LoadData();
    }

    public void TryShowInUI(string id, Transform focusTran, bool autoUpdatePos = false, CursorViewType viewType = CursorViewType.Default)
        => TryShowInUI(id, focusTran, out _, autoUpdatePos, viewType);

    public void TryShowInUI(string id, Transform focusTran, out bool successful, bool autoUpdatePos = false, CursorViewType viewType = CursorViewType.Default)
        => Show(id, focusTran, autoUpdatePos, TutorialBasingType.UI, viewType, out successful);

    public void TryShowInWorld(string id, Transform focusTran, bool autoUpdatePos = true, CursorViewType viewType = CursorViewType.Default)
        => TryShowInWorld(id, focusTran, out _, autoUpdatePos);

    public void TryShowInWorld(string id, Transform focusTran, out bool successful, bool autoUpdatePos = true, CursorViewType viewType = CursorViewType.Default)
        => Show(id, focusTran, autoUpdatePos, TutorialBasingType.World, viewType, out successful);

    private void Show(string id, Transform focusTran, bool autoUpdatePos, TutorialBasingType basingType, CursorViewType viewType, out bool successful)
    {
        successful = false;

        if (!use) return;

        if (_completedTutorials != null && _completedTutorials.Contains(id)) return;
        if (_inTutorial) BreakTutorial();
        if (showDebugs) Debug.Log("[tutor] Show Tutor " + id);

        _inTutorial = true;
        _currentTutorialID = id;
        gameObject.SetActive(true);
        cursor.gameObject.SetActive(true);

        SetCursorVisual(viewType);

        _currentTutorialBasingType = basingType;
        _autoUpdatePos = autoUpdatePos;
        tutorialCanvas.sortingOrder = basingType == TutorialBasingType.UI ? UiOrder : WorldOrder;

        _currentFocusTran = focusTran;
        UpdateCursorPosition();

        successful = true;
    }

    private void SetCursorVisual(CursorViewType cursorViewType)
    {
        if (_currentCursorViewType == cursorViewType) return;

        _currentCursorViewType = cursorViewType;
        cursor.SetTransformData(cursorViewDatas[cursorViewType]);
        cursor.SetVisibility(cursorViewType != CursorViewType.Invisible);
    }

    /// <summary>
    /// Туториал выполнен, он не будет больше показываться
    /// </summary>
    public void CompleteTutorial()
    {
        if (!InTutorial) return;
        CompleteTutorial(_currentTutorialID);
    }

    /// <summary>
    /// Туториал выполнен, он не будет больше показываться
    /// </summary>
    public void CompleteTutorial(string id)
    {
        if (showDebugs) Debug.Log("[tutor] Complete Tutor " + id);

        if (tutorStepsToAnalytics.ContainsKey(id))
        {
            AnalyticsManager.Instance.Log(AnalyticSectionType.Tutorial, tutorStepsToAnalytics[id]);
        }

        _completedTutorials.Add(id);
        HideTutor();
    }

    /// <summary>
    /// Туториал скрывается, но не считается завершённым. Он может быть показан позже
    /// </summary>
    public void BreakTutorial()
    {
        if (!InTutorial) return;

        HideTutor();
    }

    private void HideTutor()
    {
        _inTutorial = false;
        _currentTutorialID = null;
        _autoUpdatePos = false;
        gameObject.SetActive(false);
        cursor.gameObject.SetActive(false);

        _currentTutorialBasingType = null;
        _currentFocusTran = null;

        SaveData();
    }

    public void UpdateCursorPosition()
    {
        if (!_inTutorial) return;

        switch (_currentTutorialBasingType)
        {
            case TutorialBasingType.UI:
                cursor.transform.position = _currentFocusTran != null ? _currentFocusTran.position : Vector3.zero;
                break;

            case TutorialBasingType.World:
                var focusPos = _currentFocusTran != null ? _currentFocusTran.position : Vector3.zero;
                var focusScreenPos = Camera.main.WorldToScreenPoint(focusPos);
                var nguiPos = tutorCamera.ScreenToWorldPoint(focusScreenPos);

                CursorViewType cursorVisualType = CursorViewType.Default;

                if (nguiPos.y > upPos.position.y) cursorVisualType = CursorViewType.UpEdge;
                else if (nguiPos.y < bottomPos.position.y) cursorVisualType = CursorViewType.BottomEnge;
                else if (nguiPos.x > rightPos.position.x) cursorVisualType = CursorViewType.RightEdge;
                else if (nguiPos.x < leftPos.position.x) cursorVisualType = CursorViewType.LeftEdge;

                SetCursorVisual(cursorVisualType);

                nguiPos = new Vector3(
                    MathUtility.Limit(nguiPos.x, leftPos.position.x, rightPos.position.x),
                    MathUtility.Limit(nguiPos.y, bottomPos.position.y, upPos.position.y),
                    cursor.transform.position.z);
                cursor.transform.position = nguiPos;
                break;
        }
    }

    private void Update()
    {
        if (!_inTutorial) return;
        if (!_autoUpdatePos) return;

        UpdateCursorPosition();
    }

    #region SaveLoad

    public void SaveData()
    {
        SaveManager.Save(CommonData.PREFSKEY_COMPLETED_TUTORIALS, _completedTutorials);
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_COMPLETED_TUTORIALS))
        {
            _completedTutorials = SaveManager.Load<List<string>>(CommonData.PREFSKEY_COMPLETED_TUTORIALS);
        }
        else RestartTutorials();
    }

    #endregion

    public void ClearCompletedTutorials() => RestartTutorials();

    private void RestartTutorials()
    {
        _completedTutorials = new List<string>();
        SaveData();
    }

    public enum TutorialBasingType
    {
        UI,
        World
    }

    public enum CursorViewType
    {
        Default,
        UpEdge,
        BottomEnge,
        LeftEdge,
        RightEdge,
        Invisible
    }
}

public struct CursorTransformData
{
    private Vector3 rotation;
    private Vector3 scale;

    public CursorTransformData(Vector3 rotation, Vector3 scale)
    {
        this.rotation = rotation;
        this.scale = scale;
    }

    public Vector3 Rotation => rotation;
    public Vector3 Scale => scale;
}

public interface ITutorialStarter
{
    void TryShowTutorial();
}