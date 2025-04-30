using System.Collections.Generic;
using Sirenix.OdinInspector;

public class BattleSceneController : SceneController
{
    public List<BattlePos> enemyPosList = new();
    public List<BattlePos> heroPosList = new();
    private List<BattlePos> activeEnemyPos = new();
    private List<BattlePos> activeHeroPos = new();
    void Awake()
    {
        BattleData.Instance.On(BattleData.ExitBattle, ExitBattle, this);
        InitBattle();
    }

    public void InitDataLib()
    {
        var saveRuntime = new SaveSlotData();
        saveRuntime.AddHero("Hero1");
        SaveSlotData.ReplaceInstance(saveRuntime);
    }
    public void InitBattle(string level = "Level1")
    {
        // BattleData.Init(level);

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
    [Button("开始战斗")]
    public void StartBattle()
    {
        BattleData.Instance.Emit(BattleData.BattleStart);
        BattleData.Instance.Emit(BattleData.ActionNext);
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