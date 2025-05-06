using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;

public class BattleSceneController : SceneController
{
    public List<BattlePos> enemyPosList = new();
    public List<BattlePos> heroPosList = new();
    private List<BattlePos> activeEnemyPos = new();
    private List<BattlePos> activeHeroPos = new();
    private InputAction interactAction; // 定义一个 InputAction
    protected override void Awake()
    {
        base.Awake();
        BattleData.Instance.On(BattleData.ExitBattle, ExitBattle, this);
        InitBattle();
        // 初始化 InputAction，绑定到 "ESC" 键
        interactAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/escape");
        interactAction.Enable(); // 启用 InputAction
    }

    void Update()
    {
        if (interactAction.WasPressedThisFrame())
        {
            WindowManager.Instance.ShowDialog(UIDefine.UIPause);
        }
    }

    public void InitBattle()
    {
        var enemys = BattleData.Instance.Enemys;
        var heros = BattleData.Instance.Heros;
        for (int i = 0; i < enemys.Count; i++)
        {
            var enemyPos = enemyPosList.Find(e => e.posIndex == enemys[i].position);
            enemyPos.Init(enemys[i]);
            activeEnemyPos.Add(enemyPos);
        }
        for (int i = 0; i < heros.Count; i++)
        {
            heroPosList[i].Init(heros[i]);
            activeHeroPos.Add(heroPosList[i]);
        }
    }

    [Button]
    public void Clear()
    {
        foreach (var pos in activeEnemyPos)
        {
            pos.Clear();
        }
        foreach (var pos in activeHeroPos)
        {
            pos.Clear();
        }
        activeEnemyPos.Clear();
        activeHeroPos.Clear();
        BattleData.Clear();
    }

    [Button]
    public void ExitBattle()
    {
        foreach (var pos in activeEnemyPos)
        {
            pos.Clear();
        }
        foreach (var pos in activeHeroPos)
        {
            pos.Clear();
        }
        activeEnemyPos.Clear();
        activeHeroPos.Clear();
        BattleData.Clear();
        SceneLoader.Instance.LoadPreScene();
    }
}