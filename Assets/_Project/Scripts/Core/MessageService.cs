using System;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;
using TheSTAR.Sound;
using TheSTAR.GUI;
using Zenject;
using TheSTAR.GUI.Screens;

public class MessageService : MonoSingleton<MessageService>, ISaver
{
    [SerializeField] private CommonConfig commonConfig;

    private const int MaxMessagesCount = 3;

    private MessagesData savedMessagesData;

    public MessagesData SavedMessagesData => savedMessagesData;
    public int MessagesCount => savedMessagesData.messages.Count;

    public void InitService()
    {
        LoadData();
    }

    public void ShowBattleWarningMessage(int attackerCountryID, int defenderCountryID)
    {
        var prepareMessage = GuiController.Instance.FindScreen<MessageBattlePrepareScreen>();
        prepareMessage.SetInfo(attackerCountryID, defenderCountryID);
        GuiController.Instance.Show(prepareMessage);

        MessageData element = new (attackerCountryID, defenderCountryID);
        savedMessagesData.messages.Insert(0, element);
        OnAddMessage();

        SoundController.Instance.PlaySound(SoundType.Message);
    }

    public void ShowBattleResultMessage(int countryID, bool win, int groundForce)
    {
        var messageScreen = GuiController.Instance.FindScreen<MessageInfoScreen>();
        messageScreen.Init(countryID, win, groundForce);
        GuiController.Instance.Show(messageScreen);

        MessageData element = new (countryID, win, groundForce);
        savedMessagesData.messages.Insert(0, element);
        OnAddMessage();

        SoundController.Instance.PlaySound(SoundType.Message);
    }

    public void ShowWorldEventMessage(WorldEventType worldEvent)
    {
        string text = commonConfig.GetWorldEventMessage(worldEvent);
        var messageScreen = GuiController.Instance.FindScreen<MessageWorldEventScreen>();
        messageScreen.SetData(text);
        if (GuiController.Instance.CurrentScreen is not BattleScreen) GuiController.Instance.Show(messageScreen);

        MessageData element = new(MessageType.WorldEvent, text);
        savedMessagesData.messages.Insert(0, element);
        OnAddMessage();
    }

    public void ShowRebelsTutorMessage() => ShowTutorialMessage("To protect the captured country from rebels or other countries, recruit troops in your new colony.");
    public void ShowRocketTutorMessage() => ShowTutorialMessage("You can buy as many rockets as you want, but only 3 are carried into battle.");

    private void ShowTutorialMessage(string text)
    {
        var messageScreen = GuiController.Instance.FindScreen<MessageWorldEventScreen>();
        messageScreen.SetData(text);
        GuiController.Instance.Show(messageScreen);

        //MessageData element = new(MessageType.Tutorial, text);
        //savedMessagesData.messages.Insert(0, element);
        //OnAddMessage();
    }

    private void OnAddMessage()
    {
        if (savedMessagesData.messages.Count > MaxMessagesCount) savedMessagesData.messages.Remove(savedMessagesData.messages[^1]);

        SaveData();
    }

    public void SaveData()
    {
        if (savedMessagesData == null || savedMessagesData.messages.Count == 0) return;

        SaveManager.Save(CommonData.PREFSKEY_MESSAGES, savedMessagesData);
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey(CommonData.PREFSKEY_MESSAGES))
        {
            savedMessagesData = SaveManager.Load<MessagesData>(CommonData.PREFSKEY_MESSAGES);
        }
        else savedMessagesData = new MessagesData();
    }

    public enum MessageType
    {
        BattleWarning,
        BattleResult,
        WorldEvent,
        Tutorial
    }

    [Serializable]
    public class MessagesData
    {
        public List<MessageData> messages = new List<MessageData>();
    }

    [Serializable]
    public class MessageData
    {
        public MessageType messageType;

        public int attackerId;
        public int defenderId;

        public int countryId;
        public bool win;
        public int groundForce;

        public string message;

        public MessageData()
        {
        }

        public MessageData(MessageType messageType, string text)
        {
            this.messageType = messageType;
            message = text;
        }

        public MessageData(int attackerId, int defenderId)
        {
            messageType = MessageType.BattleWarning;
            this.attackerId = attackerId;
            this.defenderId = defenderId;
        }

        public MessageData(int countryId, bool win, int groundForce)
        {
            messageType = MessageType.BattleResult;
            this.countryId = countryId;
            this.win = win;
            this.groundForce = groundForce;
        }
    }
}