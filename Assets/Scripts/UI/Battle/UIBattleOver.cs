
using TMPro;
using UnityEngine.UI;

public class UIBattleOver : BaseDialog
{
    public Button btnContinue;
    public TextMeshProUGUI txtResult;
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
        }
        else
        {
            txtResult.text = "Defeat";
        }
    }

    private void OnContinueClick()
    {
        BattleData.Instance.Emit(BattleData.ExitBattle);
    }
}
