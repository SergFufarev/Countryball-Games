using UnityEngine;

namespace FunnyBlox
{
    public class CommonData
    {
        public const int PlayerID = 1000;

        public const int ItemCost = 150; // цена одной кастомки (в харде)

        // currency
        public const string PREFSKEY_MONEY = "Money";
        public static float Money;

        public const string PREFSKEY_STARS = "Stars";
        public static float Stars;

        public const string PREFSKEY_MONEY_PER_SECOND = "MoneyPerSecond";
        /// <summary>
        /// Базовое значение экономики БЕЗ экономических бонусов
        /// </summary>
        public static float MoneyPerSecondBase;

        // countries

        public const string PREFSKEY_BASE_COINTRY_ID = "BaseCountryID";
        public const string PREFSKEY_COUNTRIES_DATA = "CountriesData";
        public static int PlayerCountriesCount;

        // daily bonus
        public const string PREFSKEY_PREVIOUS_DAILY_BONUS_TIME = "PreviousDailyBonusTime";
        public const string PREFSKEY_CURRENT_DAILY_BONUS_INDEX = "CurrentDailyBonusIndex";
        public const string PREFSKEY_BONUS_INDEX_WAS_UPDATED_FOR_THIS_DAY = "BonusIndexWasUpdatedForThisDay";

        // settings
        public const string PREFSKEY_MUSIC_ON = "musicOn";
        public const string PREFSKEY_SOUNDS_ON = "soundsOn";
        public const string PREFSKEY_VIBRATION_ON = "vibrationOn";
        public const string PREFSKEY_NOTIFICATIONS_ON = "notificationsOn";
        public const string PREFSKEY_USE_HEIGHT_QUALITY = "use_height_quality";

        public static bool MusicOn;
        public static bool SoundsOn;
        public static bool VibrationsOn;
        public static bool NotificationsOn;
        public static bool UseHighQuality;

        // notifications

        public const string PREFSKEY_SENT_NOTIFICATION_BOMB_ID = "sentNotificationBomb";
        public const string PREFSKEY_SENT_NOTIFICATION_DAILY_BONUS_ID = "sentNotificationDailyBonus";

        // packs

        public const string PREFSKEY_PREMIUM_END_TIME = "PremiumEndTime";

        // intelligence

        public const string PREFSKEY_UNLOCK_INTELLIGENCE_DATE_TIME = "UnlockIntelligenceTime";
        public const string PREFSKEY_UNLOCK_WAIT_TIME_FULL = "UnlockWaitTimeFull";

        // WorldEvent

        public const string PREFSKEY_CURRENT_WORLD_EVENT = "CurrentWorldEvent";
        public const string PREFSKEY_NEXT_CHANGE_WORLD_EVENT_TIME = "NextWorldEventTime";

        // New Game +
        public const string PREFSKEY_GAME_WAS_COMPLETED = "GameWasCompleted"; // прошёл ли игрок игру полностью хотя бы раз
        public const string PREFSKEY_GAME_PLUS = "GamePlus"; // находится ли игрок в режиме New Game +
        public const string PREFSKEY_GAME_PLUS_INDEX = "GamePlusIndex"; // сколько раз игрок начал игру в New Game +

        // other
        public const string PREFSKEY_INCOME_BOOSTER_END_TIME = "IncomeBoosterEndTime";
        public const string PREFSKEY_GDPR_WAS_ACCEPTED = "GdprWasAccepted";
        public const string PREFSKEY_MESSAGES = "Messages";
        public const string PREFSKEY_COMPLETED_TUTORIALS = "CompletedTutorial";
        public const string PREFSKEY_OFFLINE_REWARD_LAST_TIME = "OfflineRewardLastTime";
        public const string PREFSKEY_BOMB_ATTACK_TIME = "PreviousBombAttackTime";
        public const string PREFSKEY_COINS_FOR_AD_CLICK_TIME = "PreviousCoinsForAdClickTime";

        public const string PREFSKEY_SPYING_COUNTRIES_COUNT = "SpyingCountriesCount";
        public const string PREFSKEY_FULL_UPGRADED_FACTORY = "FullUpgradedFactoriesCount";

        public const string PREFSKEY_PREVIOUS_SESSION_DATE_TIME = "PreviousSessionDayTime";
        public const string PREFSKEY_SESSION_INDEX = "SessionIndex";

        public const string PREFSKEY_ADS_REMOVED = "AdsRemoved";

        public const string PREFSKEY_GAME_WAS_RATED = "GameWasRated";
        public static bool GameWasRated = false;

        public const string PREFSKEY_NEED_SHOW_RATE_US_IN_TIME = "NeedShowRateUsInTime"; // запланирован ли показ рейт аза на определённое время
        public const string PREFSKEY_TIME_TO_SHOW_RATE_US = "TimeToShowRateUs"; // на какое время запланирован показ рейт аза

        // enemy attack status
        public const string PREFSKEY_CURRENT_ENEMY_ATTACK_TYPE = "CurrentEnemyAttackType";
        public const string PREFSKEY_CURRENT_ENEMY_ATTACK_STATE = "CurrentEnemyAttackState";
        public const string PREFSKEY_WAIT_TO_ENEMY_ATTACK_TIME = "WaitToEnemyAttackTime";

        public const string PREFSKEY_ENEMY_ATTACK_REVOLT_COUNTRY_ID = "EnemyAttack_Revolt_CountryID";
        public const string PREFSKEY_ENEMY_ATTACK_INVASION_ATTACKER_COUNTRY_ID = "EnemyAttack_Invasion_AttackerCountryID";
        public const string PREFSKEY_ENEMY_ATTACK_INVASION_DEFENDER_COUNTRY_ID = "EnemyAttack_Invasion_DefenderCountryID";
        public const string PREFSKEY_ENEMY_ATTACK_MUTUAL_ATTACKER_COUNTRY_ID = "EnemyAttack_Mutual_AttackerCountryID";
        public const string PREFSKEY_ENEMY_ATTACK_MUTUAL_DEFENDER_COUNTRY_ID = "EnemyAttack_Mutual_DefenderCountryID";

        // trades
        public const string PREFSKEY_TRADE_REGIONS = "TradeRegions"; // с какими регионами заключено торговое соглашение

        // customisation

        public const string PREFSKEY_CURRENT_FACE_TYPE = "CustomFaceType";
        public const string PREFSKEY_CURRENT_HAT_TYPE = "CustomHatType";
        public const string PREFSKEY_PURCHASED_HATS = "PurchasedHats"; // какие шапки покупал игрок
        public const string PREFSKEY_PURCHASED_FACES = "PurchasedFaces"; // какие лица покупал игрок

        // custom country color

        public const string PREFSKEY_USE_CUSTOM_COUNTRY_COLOR = "UseCustomCountryColor";
        public const string PREFSKEY_CUSTOM_COUNTRY_COLOR_INDEX = "CustomCountryColorIndex";

        // big battle

        public const string PREFSKEY_BIG_BATTLE_GREEN_COUNTRY_ID = "BigBattleGreenCountryID";
        public const string PREFSKEY_BIG_BATTLE_RED_COUNTRY_ID = "BigBattleRedCountryID";
        public const string PREFSKEY_BIG_BATTLE_PLAYER_PLACEMENT = "BigBattlePlayerPlacement";
        public const string PREFSKEY_BIG_BATTLE_ROCKETS_COUNT = "BigBattleRocketsCount";
        public static int RocketsCount;
        public static bool LoadGameSceneFromBigBattle = false;

        public const string PREFSKEY_DATA_UPDATED_TO_BIG_BATTLE = "DataUpdatedToBigBattle";
        public const string PREFSKEY_CURRENT_CAMERA_MODE_IN_BIG_BATTLE = "CurrentCameraModeInBigBattle";

        /// <summary>
        /// счётчик завоеваний игрока
        /// (инкрементируется при завоевании, НЕ декрементируется при потере территории)
        /// </summary>
        public const string PREFSKEY_PLAYER_OCCUPATION_COUNTER = "PlayerOccupation";
        public static int PlayerOccupationCounter;

        public const string PREFSKEY_RATE_US_LEVEL = "RateUsLevel";
        public static int RateUsLevel;

        private static readonly Vector3 ballBackOffset = Vector3.back * 2f;
        public static Vector3 BallBackOffset => ballBackOffset;

        public const string PRIVACY_URL = "https://madpixel.dev/privacy.html";
        public const string TERMS_URL = "https://madpixel.dev/privacy.html";

        public float FPS => 1 / Time.deltaTime;

        public static GameVersionType CurrentGameVersionType;
    }
}