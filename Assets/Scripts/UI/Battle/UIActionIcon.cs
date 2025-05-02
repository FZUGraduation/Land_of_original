
using UnityEngine;
using UnityEngine.UI;

public class UIActionIcon : MonoBehaviour
{
    public Image icon;
    public Image bg;
    [HideInInspector]
    public int battleID;

    public void Init(BattleCharacterData characterData)
    {
        battleID = characterData.battleID;
        Sprite iconSprite = null;
        switch (characterData)
        {
            case BattleHeroData heroData:
                iconSprite = heroData.heroConfig.icon;
                break;
            case BattleEnemyData monsterData:
                iconSprite = monsterData.enemyConfig.icon;
                break;
        }
        icon.sprite = iconSprite;
    }

    public void SetAction()
    {
        bg.color = Color.green;
    }
}
