
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeHeroSeleceItem : MonoBehaviour
{
    public Image background;
    public Image heroIcon;
    public TextMeshProUGUI heroName;
    private string heroKey;
    private Action<string> onSelect;
    private Color defaultColor = Color.white;
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(SelectHero);
    }
    public void Init(HeroConfigData data, Action<string> onSelect)
    {
        defaultColor = background.color;
        heroIcon.sprite = data.icon;
        heroName.text = data.name;
        heroKey = data.key;
        this.onSelect = onSelect;
    }

    public void SelectHero()
    {
        background.color = Color.green;
        onSelect?.Invoke(heroKey);
    }
    public void UnSelectHero()
    {
        background.color = defaultColor;
    }
}
