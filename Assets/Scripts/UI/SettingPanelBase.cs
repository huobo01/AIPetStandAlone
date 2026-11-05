using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingPanelBase : BasePanel
{
    //自动生成的控件声明
    public Button CancelBtn;
	public Button SaveBtn;
	public Slider TemperatureSlider;
	public TMP_InputField ApiKeyInput;
	public TMP_InputField EndpointInput;
	public TMP_InputField ModelInput;
	public TMP_Dropdown ProviderDropdown;
	public TMP_Dropdown ModeDropdown;
	public TextMeshProUGUI TitleText;
	public TextMeshProUGUI ProviderLabel;
	public TextMeshProUGUI ApiKeyLabel;
	public TextMeshProUGUI EndpointLabel;
	public TextMeshProUGUI ModelLabel;
	public TextMeshProUGUI TemperatureLabel;
	public TextMeshProUGUI ModeLabel;
	public Image BackgroundDim;
	

    protected virtual void Start()
    {
        //自动生成的找到对应的控件
        CancelBtn = this.transform.Find("SettingsCard/ButtonRow/CancelBtn").GetComponent<Button>();
		SaveBtn = this.transform.Find("SettingsCard/ButtonRow/SaveBtn").GetComponent<Button>();
		TemperatureSlider = this.transform.Find("SettingsCard/TemperatureSection/TemperatureSlider").GetComponent<Slider>();
		ApiKeyInput = this.transform.Find("SettingsCard/ApiKeySection/ApiKeyInput").GetComponent<TMP_InputField>();
		EndpointInput = this.transform.Find("SettingsCard/EndpointSection/EndpointInput").GetComponent<TMP_InputField>();
		ModelInput = this.transform.Find("SettingsCard/ModelSection/ModelInput").GetComponent<TMP_InputField>();
		ProviderDropdown = this.transform.Find("SettingsCard/ProviderSection/ProviderDropdown").GetComponent<TMP_Dropdown>();
		ModeDropdown = this.transform.Find("SettingsCard/ModeSection/ModeDropdown").GetComponent<TMP_Dropdown>();
		TitleText = this.transform.Find("SettingsCard/TitleText").GetComponent<TextMeshProUGUI>();
		ProviderLabel = this.transform.Find("SettingsCard/ProviderSection/ProviderLabel").GetComponent<TextMeshProUGUI>();
		ApiKeyLabel = this.transform.Find("SettingsCard/ApiKeySection/ApiKeyLabel").GetComponent<TextMeshProUGUI>();
		EndpointLabel = this.transform.Find("SettingsCard/EndpointSection/EndpointLabel").GetComponent<TextMeshProUGUI>();
		ModelLabel = this.transform.Find("SettingsCard/ModelSection/ModelLabel").GetComponent<TextMeshProUGUI>();
		TemperatureLabel = this.transform.Find("SettingsCard/TemperatureSection/TemperatureLabel").GetComponent<TextMeshProUGUI>();
		ModeLabel = this.transform.Find("SettingsCard/ModeSection/ModeLabel").GetComponent<TextMeshProUGUI>();
		BackgroundDim = this.transform.Find("BackgroundDim").GetComponent<Image>();
		

        //自动生成的进行对应控件的事件监听
        CancelBtn.onClick.AddListener(OnCancelBtnClick);
		SaveBtn.onClick.AddListener(OnSaveBtnClick);
		TemperatureSlider.onValueChanged.AddListener(OnTemperatureSliderValueChanged);
		ApiKeyInput.onValueChanged.AddListener(OnApiKeyInputValueChanged);
		EndpointInput.onValueChanged.AddListener(OnEndpointInputValueChanged);
		ModelInput.onValueChanged.AddListener(OnModelInputValueChanged);
		ProviderDropdown.onValueChanged.AddListener(OnProviderDropdownValueChanged);
		ModeDropdown.onValueChanged.AddListener(OnModeDropdownValueChanged);
		
    }

    //自动生成的对应进行监听事件的响应函数
    protected virtual void OnCancelBtnClick(){}
	protected virtual void OnSaveBtnClick(){}
	protected virtual void OnTemperatureSliderValueChanged(float value){}
	protected virtual void OnApiKeyInputValueChanged(string value){}
	protected virtual void OnEndpointInputValueChanged(string value){}
	protected virtual void OnModelInputValueChanged(string value){}
	protected virtual void OnProviderDropdownValueChanged(int value){}
	protected virtual void OnModeDropdownValueChanged(int value){}
	

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
