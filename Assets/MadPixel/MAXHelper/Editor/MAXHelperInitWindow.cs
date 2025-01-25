using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace MAXHelper {
    public class MAXHelperInitWindow : EditorWindow {
        #region Fields
        private const string CONFIGS_PATH = "Assets/MadPixel/MAXHelper/Configs/MAXCustomSettings.asset";
        private const string PACKAGE_PATH = "Assets/MadPixel/MAXHelper/Configs/MaximumPack.unitypackage";
        private const string MEDIATIONS_PATH = "Assets/MAXSdk/Mediation/";

        private List<string> MAX_VARIANT_PACKAGES = new List<string>() { "AdColony", "ByteDance", "Fyber", "Google", "InMobi", "Mintegral", "MyTarget",
            "Tapjoy", "Vungle", "Yandex" };

        private Vector2 scrollPosition;
        private static readonly Vector2 windowMinSize = new Vector2(450, 200);
        private static readonly Vector2 windowPrefSize = new Vector2(850, 400);

        private GUIStyle titleLabelStyle;
        private GUIStyle warningLabelStyle;

        private static GUILayoutOption sdkKeyLabelFieldWidthOption = GUILayout.Width(120);
        private static GUILayoutOption sdkKeyTextFieldWidthOption = GUILayout.Width(650);
        private static GUILayoutOption buttonFieldWidth = GUILayout.Width(160);
        private static GUILayoutOption adUnitLabelWidthOption = GUILayout.Width(140);
        private static GUILayoutOption adUnitTextWidthOption = GUILayout.Width(150);
        private static GUILayoutOption adMobLabelFieldWidthOption = GUILayout.Width(100);
        private static GUILayoutOption adMobUnitTextWidthOption = GUILayout.Width(280);
        private static GUILayoutOption adUnitToggleOption = GUILayout.Width(180);
        private static GUILayoutOption bannerColorLabelOption = GUILayout.Width(250);

        private MAXCustomSettings CustomSettings;
        private bool bMaxVariantInstalled;
        private bool bInitialized;
        #endregion
        
        #region Menu Item
        [MenuItem("Mad Pixel/Setup Ads", priority = 0)]
        public static void ShowWindow() {
            var Window = EditorWindow.GetWindow<MAXHelperInitWindow>("Mad Pixel. Setup Ads", true);

            Window.Setup();
        }

        private void Setup() {
            minSize = windowMinSize;
            LoadConfigFromFile();
            AddImportCallbacks();
            CheckMaxVersion();

        }
        #endregion



        #region Editor Window Lifecyle Methods

        private void OnGUI() { 
            if (CustomSettings != null) {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition, false, false)) {
                    scrollPosition = scrollView.scrollPosition;

                    GUILayout.Space(5);

                    titleLabelStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 20
                    };

                    // Draw AppLovin MAX plugin details
                    EditorGUILayout.LabelField("1. Fill in your SDK Key", titleLabelStyle);

                    DrawSDKKeyPart();

                    DrawUnitIDsPart();

                    DrawTestPart();

                    DrawInstallButtons();
                }
            }


            if (GUI.changed) {
                AppLovinSettings.Instance.SaveAsync();
                EditorUtility.SetDirty(CustomSettings);
            }
        }

        private void OnDisable() {
            AppLovinSettings.Instance.SdkKey = CustomSettings.SDKKey;

            AssetDatabase.SaveAssets();
        }


        #endregion

        #region Draw Functions
        private void DrawSDKKeyPart() {
            GUI.enabled = true;
            CustomSettings.SDKKey = DrawTextField("AppLovin SDK Key", CustomSettings.SDKKey, sdkKeyLabelFieldWidthOption, sdkKeyTextFieldWidthOption);

            using (new EditorGUILayout.VerticalScope("box")) {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                AppLovinSettings.Instance.QualityServiceEnabled = GUILayout.Toggle(AppLovinSettings.Instance.QualityServiceEnabled, "  Enable MAX Ad Review (turn this on for production build)");
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
        }

        private void DrawUnitIDsPart() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("2. Fill in your Ad Unit IDs (from MadPixel managers)", titleLabelStyle);
            using (new EditorGUILayout.VerticalScope("box")) {
                if (CustomSettings == null) {
                    LoadConfigFromFile();
                }

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                CustomSettings.bUseRewardeds = GUILayout.Toggle(CustomSettings.bUseRewardeds, "Use Rewarded Ads", adUnitToggleOption);
                GUI.enabled = CustomSettings.bUseRewardeds;
                CustomSettings.RewardedID = DrawTextField("Rewarded Ad Unit (Android)", CustomSettings.RewardedID, adUnitLabelWidthOption, adUnitTextWidthOption);
                CustomSettings.RewardedID_IOS = DrawTextField("Rewarded Ad Unit (IOS)", CustomSettings.RewardedID_IOS, adUnitLabelWidthOption, adUnitTextWidthOption);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                CustomSettings.bUseInters = GUILayout.Toggle(CustomSettings.bUseInters, "Use Interstitials", adUnitToggleOption);
                GUI.enabled = CustomSettings.bUseInters;
                CustomSettings.InterstitialID = DrawTextField("Inerstitial Ad Unit (Android)", CustomSettings.InterstitialID, adUnitLabelWidthOption, adUnitTextWidthOption);
                CustomSettings.InterstitialID_IOS = DrawTextField("Interstitial Ad Unit (IOS)", CustomSettings.InterstitialID_IOS, adUnitLabelWidthOption, adUnitTextWidthOption);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                CustomSettings.bUseBanners = GUILayout.Toggle(CustomSettings.bUseBanners, "Use Banners", adUnitToggleOption);
                GUI.enabled = CustomSettings.bUseBanners;
                CustomSettings.BannerID = DrawTextField("Banner Ad Unit (Android)", CustomSettings.BannerID, adUnitLabelWidthOption, adUnitTextWidthOption);
                CustomSettings.BannerID_IOS = DrawTextField("Banner Ad Unit (IOS)", CustomSettings.BannerID_IOS, adUnitLabelWidthOption, adUnitTextWidthOption);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                GUI.enabled = true;
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                if (CustomSettings.bUseBanners) {
                    GUILayout.Space(24);

                    CustomSettings.BannerBackground = EditorGUILayout.ColorField("Banner Background Color: ", CustomSettings.BannerBackground, bannerColorLabelOption);

                    GUILayout.Space(4);

                }

                GUILayout.EndHorizontal();

                GUI.enabled = true;
            }
        }

        private void DrawTestPart() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("3. For testing mediations: enable Mediation Debugger", titleLabelStyle);

            using (new EditorGUILayout.VerticalScope("box")) {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(4);

                if (warningLabelStyle == null) {
                    warningLabelStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 13,
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 20
                    };
                }

                ColorUtility.TryParseHtmlString("#D22F2F", out Color C);
                warningLabelStyle.normal.textColor = C;

                if (CustomSettings.bShowMediationDebugger) {
                    EditorGUILayout.LabelField("For Test builds only. Do NOT enable this option in the production build!", warningLabelStyle);
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(4);
                CustomSettings.bShowMediationDebugger = GUILayout.Toggle(CustomSettings.bShowMediationDebugger, "Show Mediation Debugger", adUnitToggleOption);
                GUILayout.EndHorizontal();
            }
        }
        

        private void DrawInstallButtons() {
            GUILayout.Space(16);
            EditorGUILayout.LabelField("4. Install our full mediations", titleLabelStyle);
            using (new EditorGUILayout.VerticalScope("box")) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUI.enabled = false;
                if (GUILayout.Button(new GUIContent("Minimum pack is installed"), buttonFieldWidth)) {
                    // nothing here
                }

                GUI.enabled = true;

                GUILayout.Space(5);
                GUILayout.Space(10);
                GUI.enabled = !bMaxVariantInstalled;
                if (GUILayout.Button(new GUIContent(bMaxVariantInstalled ? "Maximum pack is installed" : "Install maximum pack"), buttonFieldWidth)) {
                    AssetDatabase.ImportPackage(PACKAGE_PATH, true);
                    CheckMaxVersion();
                }

                GUI.enabled = true;
                GUILayout.EndHorizontal();

                if (bMaxVariantInstalled) {

                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);

                    AppLovinSettings.Instance.AdMobAndroidAppId = DrawTextField("AndroidAdMobID",
                        AppLovinSettings.Instance.AdMobAndroidAppId, adMobLabelFieldWidthOption, adMobUnitTextWidthOption);
                    AppLovinSettings.Instance.AdMobIosAppId = DrawTextField("IOSAdMobID",
                        AppLovinSettings.Instance.AdMobIosAppId, adMobLabelFieldWidthOption, adMobUnitTextWidthOption);

                    GUILayout.Space(5);
                    GUILayout.EndHorizontal();
                }
            }
        }

        private string DrawTextField(string fieldTitle, string text, GUILayoutOption labelWidth, GUILayoutOption textFieldWidthOption = null) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            EditorGUILayout.LabelField(new GUIContent(fieldTitle), labelWidth);
            GUILayout.Space(4);
            text = (textFieldWidthOption == null) ? GUILayout.TextField(text) : GUILayout.TextField(text, textFieldWidthOption);
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            return text;
        }
        #endregion

        #region Helpers
        private void LoadConfigFromFile() {
            var Obj = AssetDatabase.LoadAssetAtPath(CONFIGS_PATH, typeof(MAXCustomSettings));
            if (Obj != null) {
                CustomSettings = (MAXCustomSettings)Obj;
            } else {
                Debug.Log("CustomSettings file doesn't exist, creating a new one...");
                var Instance = MAXCustomSettings.CreateInstance("MAXCustomSettings");
                AssetDatabase.CreateAsset(Instance, CONFIGS_PATH);
            }
        }

        private void CheckMaxVersion() {
            string[] filesPaths = System.IO.Directory.GetFiles(MEDIATIONS_PATH);
            if (filesPaths != null && filesPaths.Length > 0) {
                List<string> Paths = filesPaths.ToList();
                bool bMissingPackage = false;
                foreach (string PackageName in MAX_VARIANT_PACKAGES) {
                    if (!filesPaths.Contains(MEDIATIONS_PATH + PackageName + ".meta")) {
                        bMissingPackage = true;
                        break;
                    }
                }

                bMaxVariantInstalled = !bMissingPackage;
            }
        }

        private void AddImportCallbacks() {
            AssetDatabase.importPackageCompleted += packageName => {
                Debug.Log($"Package {packageName} installed");
                CheckMaxVersion();
            };

            AssetDatabase.importPackageCancelled += packageName => {
                Debug.Log($"Package {packageName} cancelled");
            };

            AssetDatabase.importPackageFailed += (packageName, errorMessage) => {
                Debug.Log($"Package {packageName} failed");
            };
        }

        private void RemoveImportCallbacks() {

        }

        #endregion
    } 
}
