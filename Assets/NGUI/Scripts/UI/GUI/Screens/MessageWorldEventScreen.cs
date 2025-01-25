using UnityEngine;
using TMPro;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.GUI.Screens
{
    public class MessageWorldEventScreen : GuiScreen
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private PointerButton exitButton;
        [SerializeField] private PointerButton backButton;
        [SerializeField] private bool inBattle;

        private GuiController gui;

        private void Start()
        {
            backButton.gameObject.SetActive(!inBattle);
        }

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();

            exitButton.Init(OnExitClick);
            backButton.Init(OnBackClick);
        }

        public void SetData(string text) => label.text = text;

        public void OnExitClick() => gui.ShowRootScren();

        public void OnBackClick() => gui.Show<MessagesScreen>();
    }
}