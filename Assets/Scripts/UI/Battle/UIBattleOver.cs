
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleOver : BaseDialog
{
    public Button btnContinue;
    public TextMeshProUGUI txtResult;
    public GameObject winPanel;
    public GameObject losePanel;
    public Transform contentTransform;
    public GameObject itemPrefab;
    protected override void Start()
    {
        base.Start();
        btnContinue.onClick.AddListener(OnContinueClick);

    }
    public override void Init(params object[] data)
    {
        bool isWin = (bool)data[0];
        if (isWin)
        {
            txtResult.text = "Victory";

            if (SaveSlotData.Instance.passLevels.Find(e => e == BattleData.Instance.battleLevel) == null)
            {
                var battleLevelData = Datalib.Instance.GetData<BattleLevelConfigData>(BattleData.Instance.battleLevel);
                SaveSlotData.Instance.passLevels.Add(BattleData.Instance.battleLevel);
                for (int i = 0; i < contentTransform.childCount; i++)
                {
                    Destroy(contentTransform.GetChild(i).gameObject);
                }
                foreach (var item in battleLevelData.itemReward)
                {
                    var itemShow = Instantiate(itemPrefab, contentTransform);
                    itemShow.GetComponent<NodeBagItem>().InitWithConfig(item.configData, item.amount);
                    SaveSlotData.Instance.bagData.AddItem(item.configData, item.amount);
                }
            }
        }
        else
        {
            txtResult.text = "Defeat";
            MainSceneController.lastPos = Vector3.zero;
        }
        winPanel.SetActive(isWin);
        losePanel.SetActive(!isWin);
    }

    private void OnContinueClick()
    {
        BattleData.Instance.Emit(BattleData.ExitBattle);
    }
}
