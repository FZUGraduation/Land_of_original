using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITalk : BaseDialog
{
    #region 字段
    public static UITalk Instance;
    // 左侧头像
    public Image leftRoleAvatar;
    // 右侧头像
    public Image rightRoleAvatar;
    // 左侧名称显示
    public TextMeshProUGUI leftRoleName;
    // 右侧名称显示
    public TextMeshProUGUI rightRoleName;
    // 对话文本组件
    public TextMeshProUGUI dialogText;
    // 继续按钮
    public GameObject btn_Continue;
    // 对话面板
    public GameObject dialogPanel;
    // 选项按钮预制体
    public GameObject optionButtonPrefab;
    // 选项按钮的父对象
    public GameObject optionButtonRoot;
    [HideInInspector]
    public TalkConfigData talkConfigData;
    [HideInInspector]// 当前对话ID
    public string dialogKey;
    [HideInInspector]// 当前对话索引
    public int dialogIndex;
    // 对话打字机速度
    public float tyepSpeed;
    [Sirenix.OdinInspector.ReadOnly]// 打字是否完成
    public bool isTypeDone;
    #endregion

    #region Mono
    protected override void Start()
    {
        base.Start();
        btn_Continue.GetComponent<Button>().onClick.AddListener(OnClickNextDialog);
        Instance = this;
        SetDialogID(dialogKey);
    }
    #endregion

    #region Public

    public override void Init(params object[] data)
    {
        dialogKey = data[0].ToString();
    }

    /// <summary>
    /// 显示对话面板，然后调用此函数即可启动对话系统
    /// </summary>
    public void SetDialogID(string key)
    {
        dialogKey = key;
        talkConfigData = Datalib.Instance.GetData<TalkConfigData>(key);
        dialogIndex = 0;
        UpdateDialog();
    }

    /// <summary>
    /// 继续对话
    /// </summary>
    public void OnClickNextDialog()
    {
        if (isTypeDone)
            UpdateDialog();
    }

    /// <summary>
    /// 选项分支的响应函数
    /// </summary>
    /// <param name="nextID">下一个ID</param>
    private void OnClickOptionDialog(TalkOption option)
    {
        // 点击之后销毁 所有选项按钮
        foreach (Transform t in optionButtonRoot.transform)
        {
            Destroy(t.gameObject);
        }

        switch (option.optionType)
        {
            case TalkOptionType.JumpTalk:
                SetDialogID(option.nextTalk.key);
                break;
            case TalkOptionType.Continue:
                OnClickNextDialog();
                break;
            case TalkOptionType.End:
                Close();
                break;
            case TalkOptionType.Battle:
                // 进入战斗
                BattleData.Init(option.battle.key);
                SceneLoader.Instance.LoadScene(SceneLoader.battleScene);
                break;
        }
    }

    #endregion

    #region Private
    /// <summary>
    /// 更新对话
    /// </summary>
    private void UpdateDialog()
    {
        if (dialogIndex >= talkConfigData.talkDataList.Count)
        {
            Debug.Log("对话索引越界，对话结束");
            Close();
            return;
        }
        var talkData = talkConfigData.talkDataList[dialogIndex];

        // Dialog类型处理
        if (talkData.type == TalkType.Dialogue)
        {
            UpdateText(talkData.talker.key, talkData.content, talkData.isLeft);
            UpdateImage(talkData.talker.icon, talkData.isLeft);
            btn_Continue.SetActive(true);
            dialogIndex++;
        }
        // Choose类型处理
        else if (talkData.type == TalkType.Option)
        {
            btn_Continue.SetActive(false);
            GenerateOption(dialogIndex);
            dialogIndex++;
        }
        // End类型处理
        else if (talkData.type == TalkType.End)
        {
            // dialogPanel.SetActive(false);
            Close();
        }
    }

    /// <summary>
    /// 生成对话选项按钮
    /// </summary>
    /// <param name="index">当前行数据</param>
    private void GenerateOption(int index)
    {
        var talkData = talkConfigData.talkDataList[index];

        for (int i = 0; i < talkData.optionList.Count; i++)
        {
            var option = talkData.optionList[i];
            // 生成按钮
            GameObject optionBtn = Instantiate(optionButtonPrefab, optionButtonRoot.transform);
            // 填入文本
            optionBtn.GetComponentInChildren<TextMeshProUGUI>().text = option.content;
            // 绑定事件
            optionBtn.GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    OnClickOptionDialog(option);
                });
        }
    }


    /// <summary>
    /// 更新角色对话的名称和内容
    /// </summary>
    public void UpdateText(string name, string text, bool leftOrRight)
    {
        if (leftOrRight)
        {
            leftRoleName.gameObject.SetActive(true);
            rightRoleName.gameObject.SetActive(false);
            leftRoleName.text = name;
        }
        else
        {
            leftRoleName.gameObject.SetActive(false);
            rightRoleName.gameObject.SetActive(true);
            rightRoleName.text = name;
        }

        StartCoroutine(Typing_Y(text));
    }

    /// <summary>
    /// 更新角色立绘或头像
    /// </summary>
    public void UpdateImage(Sprite sprite, bool leftOrRight)
    {
        if (leftOrRight)
        {
            leftRoleAvatar.gameObject.SetActive(true);
            rightRoleAvatar.gameObject.SetActive(false);
            leftRoleAvatar.sprite = sprite;
        }
        else
        {
            leftRoleAvatar.gameObject.SetActive(false);
            rightRoleAvatar.gameObject.SetActive(true);
            rightRoleAvatar.sprite = sprite;
        }
    }


    #endregion

    // 打字机协程
    IEnumerator Typing_Y(string content)
    {
        isTypeDone = false;
        dialogText.text = "";
        for (int i = 0; i < content.Length; i++)
        {
            dialogText.text += content[i];
            yield return new WaitForSeconds(tyepSpeed);
        }
        isTypeDone = true;
    }
}


