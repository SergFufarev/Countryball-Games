using System;
using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility;
using Zenject;
using MAXHelper;
using FunnyBlox;
using TheSTAR.GUI.UniversalElements;

namespace TheSTAR.GUI
{
    public abstract class GuiController : MonoBehaviour, IController
    {
        #region Inspector

        [SerializeField] private GuiScreen[] screens = new GuiScreen[0];
        [SerializeField] private GuiScreen mainScreen;

        [Header("Universal")]
        [SerializeField] private GuiUniversalElement[] universalElements = new GuiUniversalElement[0];

        [Space]
        [SerializeField] private Camera uiCamera;

        #endregion // Inspector

        [Inject] protected readonly TutorContainer tutor;
        [Inject] protected readonly TimeController time;

        public TutorContainer TutorContainer => tutor; // todo полностью отделить туторы, вынести в TutorialController как сделано в Кайдзю

        private ControllerStorage controllerStorage;
        private GuiScreen currentScreen;
        private GuiScreen currentRootScreen;
        private List<GuiUniversalElement> currentUniversalElements;

        public GuiScreen CurrentScreen => currentScreen;
        public Camera UiCamera => uiCamera;
        public List<GuiUniversalElement> CurrentUniversalElements => currentUniversalElements;

        private List<GuiScreen> _screensHistory = new();

        private static GuiController instance;
        public static GuiController Instance => instance;

        private void Awake()
        {
            instance = this;
        }

        public void Init(
            out List<IUpgradeReactable> urs,
            out List<ITransactionReactable> trs)
        {
            currentUniversalElements = new();

            IController[] controllers = PackControllers();
            Array.Sort(controllers);
            controllerStorage = new (controllers);

            urs = new();
            trs = new();

            InitGroup(screens, urs, trs);
            InitGroup(universalElements, urs, trs);

            void InitGroup<T>(
                T[] group,
                List<IUpgradeReactable> urs,
                List<ITransactionReactable> trs) where T : GuiObject
            {
                for (int i = 0; i < group.Length; i++)
                {
                    var e = group[i];

                    if (e == null) continue;

                    if (e is IUpgradeReactable ur) urs.Add(ur);
                    if (e is ITransactionReactable tr) trs.Add(tr);

                    e.Init(controllerStorage);
                }
            }
        }

        protected abstract IController[] PackControllers();

        public void ShowRootScren()
        {
            if (currentRootScreen) Show(currentRootScreen, true);
            else ShowMainScreen();
        }

        public void ShowMainScreen() => Show(mainScreen, true);

        public void Show<TScreen>(bool closeCurrentScreen = true, Action endAction = null, bool skipShowAnim = false) where TScreen : GuiScreen
        {
            if (CurrentScreen is TScreen) return;

            GuiScreen screen = FindScreen<TScreen>();
            Show(screen, closeCurrentScreen, endAction, skipShowAnim);
        }

        public void Show<TScreen>(TScreen screen, bool closeCurrentScreen = true, Action endAction = null, bool skipShowAnim = false) where TScreen : GuiScreen
        {
            if (CurrentScreen != null && CurrentScreen == screen && currentScreen.IsShow) return;
            if (!screen) return;

            if (closeCurrentScreen && currentScreen) currentScreen.Hide(ShowNextScreen);
            else ShowNextScreen();

            void ShowNextScreen()
            {
                screen.Show(endAction, skipShowAnim);
                currentScreen = screen;

                if (currentScreen.Root)
                {
                    currentRootScreen = currentScreen;
                    _screensHistory.Clear();
                }

                if (_screensHistory.Contains(screen))
                {
                    // сбрасываем историю до этого экрана
                    for (int i = _screensHistory.Count - 1; i >= 0; i--)
                    {
                        if (_screensHistory[i] == screen) break;
                        else _screensHistory.Remove(_screensHistory[i]);
                    }
                }
                else
                {
                    _screensHistory.Add(screen);
                }

                if (screen.Pause) time.Stop();
                else time.Play();

                if (CameraService.Instance)
                {
                    if (screen.DraggableCamera) CameraService.Instance.PlayLeanDragCamera();
                    else CameraService.Instance.StopLeanDragCamera();
                }

                if (universalElements != null && universalElements.Length > 0)
                {
                    var counters = FindUniversalElement<IncomeContainer>();
                    if (UpdateUniversalPanel(counters, screen.UseCounters))
                    {
                        counters.InitVisual(screen.UseSoftCounter, screen.UseHardCounter, screen.UseSoftPerSecondCounter, screen.UseIncomeIncreaseCounter);
                    }
                }

                bool UpdateUniversalPanel<T>(T universalElement, bool needShow) where T : GuiUniversalElement
                {
                    var element = FindUniversalElement<T>();
                    if (needShow)
                    {
                        element.Show();
                        if (!currentUniversalElements.Contains(element)) currentUniversalElements.Add(element);
                    }
                    else
                    {
                        element.Hide();
                        if (currentUniversalElements.Contains(element)) currentUniversalElements.Remove(element);
                    }

                    return needShow;
                }
            }
        }

        public void Show(Type screenType)
        {
            GuiScreen screen = FindScreen(screenType);
            Show(screen);
        }

        public void HideCurrentScreen(Action endAction = null)
        {
            if (!currentScreen) return;
            currentScreen.Hide(endAction);
        }

        public void HideCurrentUniversalElements()
        {
            if (currentUniversalElements == null || currentUniversalElements.Count == 0) return;

            foreach (var ue in currentUniversalElements) ue.Hide();
        }

        public void ShowCurrentUniversalElements()
        {
            if (currentUniversalElements == null || currentUniversalElements.Count == 0) return;

            foreach (var ue in currentUniversalElements) ue.Show();
        }

        // todo use screens history
        public void Exit()
        {
            if (CurrentScreen == null || CurrentScreen.Root) return;

            HideCurrentScreen(() =>
            {
                if (_screensHistory.Count > 1) Show(_screensHistory[^2]);
                else ShowRootScren();
            });
        }

        public T FindScreen<T>() where T : GuiScreen
        {
            int index = ArrayUtility.FastFindElement<GuiScreen, T>(screens);

            if (index == -1)
            {
                Debug.LogError($"Not found screen {typeof(T)}");
                return null;
            }
            else return (T)(screens[index]);
        }

        public GuiScreen FindScreen(Type screenType)
        {
            foreach (var screen in screens)
            {
                if (screen.GetType() == screenType) return screen;
            }

            return null;
        }

        public T FindUniversalElement<T>() where T : GuiUniversalElement
        {
            int index = ArrayUtility.FastFindElement<GuiUniversalElement, T>(universalElements);

            if (index == -1)
            {
                Debug.LogError($"Not found universal element {typeof(T)}");
                return null;
            }
            else return (T)(universalElements[index]);
        }

#if UNITY_EDITOR
        [ContextMenu("SortScreens")]
        private void SortScreens()
        {
            Array.Sort(screens);
            Array.Sort(universalElements);
        }

        [ContextMenu("GetScreens")]
        private void GetScreens()
        {
            screens = GetComponentsInChildren<GuiScreen>(true);
        }
#endif

        public void GoToRootScreen() => ShowMainScreen();

        #region Comparable

        public int CompareTo(IController other) => ToString().CompareTo(other.ToString());
        public int CompareToType<T>() where T : IController => ToString().CompareTo(typeof(T).ToString());

        #endregion
    }
}