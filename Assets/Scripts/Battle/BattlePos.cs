using UnityEngine;

public class BattlePos : MonoBehaviour
{
    public int posIndex = 0;
    private BattleCharacterData character;
    private GameObject characterGo;
    private bool isInit = false;
    public void Init(BattleCharacterData character)
    {
        if (isInit)
        {
            Debug.LogError("BattlePos has been initialized");
            return;
        }
        isInit = true;
        this.character = character;
        switch (character)
        {
            case BattleHeroData hero:
                InitHero(hero);
                break;
            case BattleEnemyData enemy:
                InitEnemy(enemy);
                break;
        }
    }
    public void InitHero(BattleHeroData heroData)
    {
        var prefab = heroData.heroConfig.prefab;
        characterGo = Instantiate(prefab, transform);
        var baseHero = characterGo.GetComponent<BaseHero>();
        baseHero.Init(heroData);
        heroData.characterMono = baseHero;
    }

    public void InitEnemy(BattleEnemyData enemyData)
    {
        var prefab = enemyData.enemyConfig.prefab;
        characterGo = Instantiate(prefab, transform);
        var baseEnemy = characterGo.GetComponent<BaseEnemy>();
        baseEnemy.Init(enemyData);
        enemyData.characterMono = baseEnemy;
    }
    public void Clear()
    {
        if (characterGo != null)
        {
            characterGo.GetComponent<BaseCharacter>().GetBtData().OnStop();
            Destroy(characterGo);
        }
        character = null;
        isInit = false;
    }
}
