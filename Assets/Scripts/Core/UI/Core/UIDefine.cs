using System.Collections.Generic;

public enum UIDefine
{
    NULL,
    UIMainScene,
    UIBattle,
    UIBattleOver,
    UISlotSelect,
    UIStartMenu,
    UIStartHeroSelect,
    UITalent,
    UITalk,
    UIBag,
    UICharacterShow,
    UIEnemyDetail,

    UIConfirmBox,
}
public static class UIDefineExtensions
{
    private static readonly Dictionary<UIDefine, string> _enumToString = new Dictionary<UIDefine, string>
    {
        { UIDefine.NULL, "NULL" },
        { UIDefine.UIMainScene, "Prefabs/UI/MainScene/UIMainScene" },
        { UIDefine.UIBattle, "Prefabs/UI/Battle/UIBattle" },
        { UIDefine.UIBattleOver, "Prefabs/UI/Battle/UIBattleOver" },
        { UIDefine.UISlotSelect, "Prefabs/UI/StartScene/UISlotSelect" },
        { UIDefine.UIStartMenu, "Prefabs/UI/StartScene/UIStartMenu" },
        { UIDefine.UIStartHeroSelect, "Prefabs/UI/StartScene/UIStartHeroSelect" },
        { UIDefine.UITalk, "Prefabs/UI/Talk/UITalk" },
        { UIDefine.UITalent, "Prefabs/UI/Talent/UITalent" },
        { UIDefine.UIBag, "Prefabs/UI/Bag/UIBag" },
        { UIDefine.UICharacterShow, "Prefabs/UI/Common/UICharacterShow" },
        { UIDefine.UIEnemyDetail, "Prefabs/UI/Battle/UIEnemyDetail" },

        { UIDefine.UIConfirmBox, "Prefabs/UI/Common/UIConfirmBox" },
        // 其他枚举值对应的字符串
    };

    public static string GetDialogPath(this UIDefine enumValue)
    {
        return _enumToString[enumValue];
    }
}