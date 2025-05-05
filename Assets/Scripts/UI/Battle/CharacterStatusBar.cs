using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatusBar : MonoBehaviour
{
    public Transform effectRoot;
    public GameObject effectPrefab;
    public Slider hpSlider;
    public TextMeshProUGUI hpText;
    private List<(BaseEffect, NodeEffectIem)> effectItems = new();

    private Transform targetTrs;
    void Awake()
    {
        foreach (Transform child in effectRoot)
        {
            Destroy(child.gameObject);
        }
    }
    void Update()
    {
        if (targetTrs == null)
        {
            return;
        }
        Vector3 pos = Camera.main.WorldToScreenPoint(targetTrs.position);
        transform.position = pos;
    }

    public void Init(Transform targetTrs)
    {
        this.targetTrs = targetTrs;
    }

    public void SetHp(float hp, float maxHp)
    {
        hpSlider.value = hp / maxHp;
        hpText.text = $"{(int)hp}/{(int)maxHp}";
    }
    public void AddEffect(BaseEffect effect)
    {
        if (effectItems.FindIndex((x) => x.Item1 == effect) != -1 || effect.icon == null)
        {
            return;
        }

        var effectItem = Instantiate(effectPrefab, effectRoot).GetComponent<NodeEffectIem>();
        effectItem.Init(effect);
        effectItems.Add((effect, effectItem));
    }
    public void RemoveEffect(BaseEffect modifier)
    {
        int index = effectItems.FindIndex((x) => x.Item1 == modifier);
        if (index == -1)
        {
            return;
        }
        Destroy(effectItems[index].Item2.gameObject);
        effectItems.RemoveAt(index);
    }
}
