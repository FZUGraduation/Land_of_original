using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPause : BaseDialog
{
    public Slider bgmSlider;
    public Slider seSlider;
    public Button exitButton;
    public Button backButton;

    protected override void Start()
    {
        base.Start();
        bgmSlider.onValueChanged.AddListener(OnBGMValueChange);
        seSlider.onValueChanged.AddListener(OnSEValueChange);
        exitButton.onClick.AddListener(OnExitClick);
        backButton.onClick.AddListener(OnBackClick);
    }
    public override void Init(params object[] data)
    {
        bgmSlider.value = SaveSlotData.Instance.bgmVolum;
        seSlider.value = SaveSlotData.Instance.seVolum;
    }

    private void OnBGMValueChange(float value)
    {
        SaveSlotData.Instance.bgmVolum = value;
        AudioManager.Instance.SetVolumeBGM(value);
    }

    private void OnSEValueChange(float value)
    {
        SaveSlotData.Instance.seVolum = value;
        AudioManager.Instance.SetVolumeSE(value);
    }

    private void OnExitClick()
    {
        string sceneName = SceneLoader.Instance.CurrSceneName;
        if (sceneName == SceneLoader.mainScene)
        {
            SceneLoader.Instance.LoadScene(SceneLoader.startScene);
        }
        else if (sceneName == SceneLoader.battleScene)
        {
            BattleData.Instance.Emit(BattleData.BattleEnd, false);
            Close();
        }
    }

    private void OnBackClick()
    {

    }
}
