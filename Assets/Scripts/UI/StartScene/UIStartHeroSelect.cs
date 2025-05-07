

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStartHeroSelect : BaseDialog
{
    public Transform heroItemRoot;
    public Button confirmButton;
    private string selectHeroKey = "";
    private Dictionary<string, NodeHeroSeleceItem> heroItemDic = new();
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        var heros = Datalib.Instance.GetDatas<HeroConfigData>();
        for (int i = 0; i < heros.Count; i++)
        {
            if (i == 1 || heros[i].isPlayer == false) continue; //跳过第二个英雄
            var slot = Instantiate(Resources.Load<GameObject>("Prefabs/UI/StartScene/NodeHeroSeleceItem"), heroItemRoot);
            slot.name = $"HeroSelectItem_{i}";
            var heroItem = slot.GetComponent<NodeHeroSeleceItem>();
            heroItemDic.Add(heros[i].key, heroItem);
            heroItem.Init(heros[i], OnHeroSelect);
            if (i == 0)
            {
                heroItem.SelectHero();//默认选择第一个
            }
        }
        confirmButton.onClick.AddListener(OnConfirm);
    }

    protected override void OnClose()
    {
        base.OnClose();
    }

    private void OnHeroSelect(string heroKey)
    {
        if (heroKey == selectHeroKey)
        {
            return;
        }
        if (heroItemDic.ContainsKey(selectHeroKey))
        {
            heroItemDic[selectHeroKey].UnSelectHero();
        }
        Debug.Log("选择英雄：" + heroKey);
        selectHeroKey = heroKey;
    }

    private void OnConfirm()
    {
        SaveSlotData.Instance.AddHero(selectHeroKey);
        SaveSlotData.Instance.MarkStoryProgress(StoryProgress.heroSelect);
        SceneLoader.Instance.LoadScene(SceneLoader.mainScene);
    }
}
