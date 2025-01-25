using UnityEngine;
using FunnyBlox;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;
using TheSTAR.Utility.Pointer;
using TMPro;

public class MessageUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI subtitle;
    [SerializeField] private PointerButton infoButton;

    MessageService.MessageData _data;

    private GuiController gui;
    private VisualCountryController countries;

    public void Init(ControllerStorage cts)
    {
        gui = cts.Get<GuiController>();
        countries = cts.Get<VisualCountryController>();

        infoButton.Init(OnMessageInfoClick);
    }

    public void SetInfo(MessageService.MessageData data)
    {
        _data = data;

        switch (data.messageType)
        {
            case MessageService.MessageType.BattleWarning:
                title.text = "Battle warning";
                subtitle.text = $"{countries.GetCountry(data.attackerId).LocalCountryData.Name} -> {countries.GetCountry(data.defenderId).LocalCountryData.Name}";
                break;

            case MessageService.MessageType.BattleResult:
                title.text = "War report";
                subtitle.text = data.win ? "You won!" : "You lose";
                break;

            case MessageService.MessageType.WorldEvent:
                title.text = "World Event";
                subtitle.text = "";
                break;

            case MessageService.MessageType.Tutorial:
                title.text = "Message";
                subtitle.text = "";
                break;
        }
    }

    public void OnMessageInfoClick()
    {
        switch (_data.messageType)
        {
            case MessageService.MessageType.BattleWarning:
                var prepareMessage = gui.FindScreen<MessageBattlePrepareScreen>();
                prepareMessage.SetInfo(_data.attackerId, _data.defenderId);
                gui.Show(prepareMessage);
                break;

            case MessageService.MessageType.BattleResult:
                var messageScreen = gui.FindScreen<MessageInfoScreen>();
                messageScreen.Init(_data.countryId, _data.win, _data.groundForce);
                gui.Show(messageScreen);
                break;

            case MessageService.MessageType.WorldEvent:
                var worldEventMessage = gui.FindScreen<MessageWorldEventScreen>();
                worldEventMessage.SetData(_data.message);
                gui.Show(worldEventMessage);
                break;

            case MessageService.MessageType.Tutorial:
                var tutorMessage = gui.FindScreen<MessageWorldEventScreen>();
                tutorMessage.SetData(_data.message);
                gui.Show(tutorMessage);
                break;
        }
    }
}