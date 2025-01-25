using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility.Pointer;
using Battle;
using TheSTAR.Sound;

namespace TheSTAR.GUI.Screens
{
    public class BattleWinScreen : GuiScreen
    {
        [SerializeField] private PointerButton okButton;

        private BattleController battle;
        //private Sound.SoundController sounds;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);

            battle = cts.Get<BattleController>();
            //sounds = cts.Get<Sound.SoundController>();

            okButton.Init(OnOkClick);
        }

        private void OnOkClick()
        {
            battle.GoToGameScene();
        }
    }
}