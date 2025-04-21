using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NodeEffectIem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TextMeshProUGUI stayCountText;
    private BaseEffect effect;

    public void Init(BaseEffect effect)
    {
        icon.sprite = effect.icon;
        stayCountText.text = effect.StayCount.ToString();
        effect.OnStayCountChange += OnStayCountChange;
        this.effect = effect;
    }

    private void OnStayCountChange(int stayCount)
    {
        stayCountText.text = stayCount.ToString();
    }
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        BattleData.Instance.Emit(BattleData.ShowDetail, effect, transform.position + new Vector3(0, 50, 0));
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        BattleData.Instance.Emit(BattleData.HideDetail);
    }
}
