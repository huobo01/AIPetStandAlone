using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinimalModePanelBase : BasePanel
{
    //自动生成的控件声明
    public Button SendBtn;
	public TMP_InputField ChatInputField;
	public TextMeshProUGUI Bubble_Receiver;
	public TextMeshProUGUI Bubble_Sender;
	public Image AvatarFrame;
	public Image PetAvatar;
	public Image Bubble_Receiver_Bg;
	public Image Bubble_Sender_Bg;
	

    protected virtual void Start()
    {
        //自动生成的找到对应的控件
        SendBtn = this.transform.Find("ChatInputBar/SendBtn").GetComponent<Button>();
		ChatInputField = this.transform.Find("ChatInputBar/ChatInputField").GetComponent<TMP_InputField>();
		Bubble_Receiver = this.transform.Find("ChatBubbleContainer/Bubble_Receiver_Bg/Bubble_Receiver").GetComponent<TextMeshProUGUI>();
		Bubble_Sender = this.transform.Find("ChatBubbleContainer/Bubble_Sender_Bg/Bubble_Sender").GetComponent<TextMeshProUGUI>();
		AvatarFrame = this.transform.Find("AvatarFrame").GetComponent<Image>();
		PetAvatar = this.transform.Find("AvatarFrame/PetAvatar").GetComponent<Image>();
		Bubble_Receiver_Bg = this.transform.Find("ChatBubbleContainer/Bubble_Receiver_Bg").GetComponent<Image>();
		Bubble_Sender_Bg = this.transform.Find("ChatBubbleContainer/Bubble_Sender_Bg").GetComponent<Image>();
		

        //自动生成的进行对应控件的事件监听
        SendBtn.onClick.AddListener(OnSendBtnClick);
		ChatInputField.onValueChanged.AddListener(OnChatInputFieldValueChanged);
		
    }

    //自动生成的对应进行监听事件的响应函数
    protected virtual void OnSendBtnClick(){}
	protected virtual void OnChatInputFieldValueChanged(string value){}
	

    //面板显示（子类可重写）
    public override void ShowMe()
    {
        gameObject.SetActive(true);
    }

    //面板隐藏（子类可重写）
    public override void HideMe()
    {
        gameObject.SetActive(false);
    }

}
