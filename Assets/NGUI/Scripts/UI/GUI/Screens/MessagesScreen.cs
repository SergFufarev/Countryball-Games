using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility.Pointer;
using FunnyBlox;

namespace TheSTAR.GUI.Screens
{
    public class MessagesScreen : GuiScreen
    {
        [SerializeField] private MessageUIElement[] messages;
        [SerializeField] private GameObject emptyTitleObject;
        [SerializeField] private PointerButton closeButton;

        private GuiController gui;

        public override void Init(ControllerStorage cts)
        {
            base.Init(cts);
            gui = cts.Get<GuiController>();

            foreach (var element in messages) element.Init(cts);

            closeButton.Init(OnCloseClick);
        }

        public void OnCloseClick() => gui.Exit();

        protected override void OnShow()
        {
            base.OnShow();

            int savedMessagesCount = MessageService.Instance.MessagesCount;

            int i = 0;
            for (; i < messages.Length && i < savedMessagesCount; i++)
            {
                messages[i].gameObject.SetActive(true);
                var data = MessageService.Instance.SavedMessagesData.messages[i];
                messages[i].SetInfo(data);
            }
            for (; i < messages.Length; i++) messages[i].gameObject.SetActive(false);

            emptyTitleObject.SetActive(savedMessagesCount <= 0);
        }
    }
}