using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour {
	public GameObject settingPanel;
	public GameObject gameInfoPanel;
	public Text levelLabel;
	public Slider levelSlider;
	public Slider effectSlider;

	void Start()
	{
		levelLabel.text = ""+Tool.getInstance().AILevel;
		levelSlider.value = Tool.getInstance().AILevel-1;
		effectSlider.value = Tool.getInstance().effectVolume;
	}

	public void twoPlayerGame()
	{
		Application.LoadLevel(1);
		Tool.getInstance().gameType = 0;
	}

	public void singlePlayerGame()
	{
		Application.LoadLevel(1);
		Tool.getInstance().gameType = 1;
	}

	public void GameSetting()
	{
		settingPanel.SetActive(true);
	}

	public void GameSettingClose()
	{
		settingPanel.SetActive(false);
	}

	public void GameInfoPanel(){
		gameInfoPanel.SetActive(true);
	}

	public void GameInfoPanelClose(){
		gameInfoPanel.SetActive(false);
	}

	public void changeAILevel(float level)
	{
		levelLabel.text = ""+(level+1);
		Tool.getInstance().AILevel = (int)(level+1);
	}

	public void changeBackground(float volume)
	{

	}

	public void changeEffect(float volume)
	{
		Tool.getInstance().effectVolume = volume;
	}
}
