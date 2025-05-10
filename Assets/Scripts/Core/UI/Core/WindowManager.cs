using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Threading.Tasks;

public enum UIIndex
{
    ROOT = 1, //最底层
    STACK = 100, // 界面层(堆栈管理)
    LOADING = 2000, // load加载界面层
    TOAST = 3000, // 提示层(toast)
    GUILD = 4000, // 引导层
    TOP = 10000, //顶层 baner界面、断线重连提示界面等,最上层
}

public class WindowManager : Singleton<WindowManager>
{
    /// <summary>存储所有UI界面</summary>
    private List<BaseDialog> uiDialogues = new();
    /// <summary>用来标记已经加载的界面</summary>
    private Dictionary<UIDefine, bool> dialogCache = new();

    private GameObject dialogCanvas;
    // private bool isOpeningDialog = false;
    public WindowManager()
    {
        FrameEvent.Instance.On(FrameEvent.AfterSceneLoder, OnSceneChange, this);
    }

    public void ShowDialog(UIDefine uiType, UIIndex uiIndex = UIIndex.STACK, params object[] data)
    {
        _ = ShowDialogAsync(uiType, uiIndex, data);
    }

    public async UniTask<GameObject> ShowDialogAsync(UIDefine uiType, UIIndex uiIndex = UIIndex.STACK, params object[] data)
    {
        if (dialogCache.ContainsKey(uiType) && dialogCache[uiType])
        {
            Debug.LogError(uiType + " 正在打开中");
            return null;
        }
        string dialogPath = uiType.GetDialogPath();
        string dialogName = uiType.ToString();
        if (string.IsNullOrEmpty(dialogPath))
        {
            Debug.LogError("dialogPath is empty");
            return null;
        }

        // 看能不能找到已经打开的窗口
        var dialogGO = GetDlgParent().transform.Find(dialogName)?.gameObject;
        var control = dialogGO?.GetComponent<BaseDialog>() ?? null;
        bool canMultiple = control?.canMultiple ?? false;
        if (dialogGO && control && !canMultiple)
        {
            Debug.LogError("窗口已经打开:" + dialogPath);
            return null;
        }

        if (dialogGO == null || canMultiple)
        {
            // isOpeningDialog = true;
            dialogGO = await Resources.LoadAsync<GameObject>(dialogPath) as GameObject;
            if (dialogGO == null)
            {
                // isOpeningDialog = false;
                Debug.LogError("Failed to load dialog: " + dialogName);
                return null;
            }
            else
            {
                if (!canMultiple) dialogCache[uiType] = true;
            }
            dialogGO = GameObject.Instantiate(dialogGO);
        }
        dialogGO.SetActive(true);
        // if (dialogGO.name != dialogName)
        // {
        //     Debug.LogError($"窗口名与文件名不一致 path = {dialogPath} name = {dialogGO.name}");
        //     dialogGO.name = dialogName;
        // }
        // isOpeningDialog = false;
        dialogGO.name = dialogName;
        control = dialogGO.GetComponent<BaseDialog>();
        if (control)
        {
            PushDialogue(control, uiIndex);
            try
            {
                control.Init(data);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Init UIDialog Error: " + e);
            }
        }
        else
        {
            Debug.LogError("Can't find BaseDialog component in " + dialogName);
            return null;
        }
        dialogCache[uiType] = false;
        Debug.Log("打开窗口:" + dialogPath);
        return dialogGO;
    }

    private void PushDialogue(BaseDialog dialog, UIIndex uiIndex = UIIndex.STACK)
    {
        int index = uiDialogues.FindIndex((item) => item == dialog);
        bool isShowAgain = false;
        if (index != -1)
        {
            uiDialogues.RemoveAt(index);
            isShowAgain = true;
        }
        index = uiDialogues.Count - 1;
        // bool showPreDialog = dialog.showPreDialog;
        // while (showPreDialog && index >= 0)
        // {
        //     showPreDialog = uiDialogues[index].showPreDialog;
        //     index--;
        // }
        for (int i = index; i >= 0; i--)
        {
            if (uiDialogues[i].gameObject == null)
            {
                uiDialogues.RemoveAt(i);
            }
        }
        var preControl = uiDialogues.Count > 0 ? uiDialogues[uiDialogues.Count - 1] : null;
        if (preControl && preControl.needHide && dialog.needHideOther)
        {
            preControl.OnBeHide();
        }
        uiDialogues.Add(dialog);
        if (isShowAgain)
        {
            dialog.OnResume();
        }
        else
        {
            if (uiIndex == UIIndex.STACK)
            {
                dialog.transform.SetParent(GetDlgParent().transform, false);
            }
        }
    }

    private void PopDialogue(BaseDialog dialog)
    {
        foreach (var item in uiDialogues)
        {
            if (item == null)
            {
                uiDialogues.Remove(item);
            }
        }
        int index = uiDialogues.FindIndex((item) => item == dialog);
        if (index != -1)
        {
            if (index > 0)
            {
                uiDialogues[index - 1]?.OnBackToTop();
            }
            GameObject.Destroy(dialog.gameObject);
            Debug.Log("关闭窗口:" + dialog.name);
            uiDialogues.RemoveAt(index);
        }
        else
        {
            Debug.LogError($"POP Dialogue:Can't find dialog:{dialog.name} in uiDialogues to");
            return;
        }
    }

    public void ClearAllDialog()
    {
        uiDialogues.Clear();
        dialogCache.Clear();
        dialogCanvas = null;
    }

    public void OnSceneChange(object[] args)
    {
        string sceneName = args[0] as string;
        if (sceneName != SceneLoader.loadingScene)
        {
            ClearAllDialog();
        }
    }

    /// <summary>获取界面层的父节点</summary>
    public GameObject GetDlgParent()
    {
        if (dialogCanvas != null)
        {
            return dialogCanvas;
        }
        dialogCanvas = GameObject.Find("DialogCanvas");
        if (dialogCanvas == null)
        {
            dialogCanvas = new GameObject("DialogCanvas");
            dialogCanvas.layer = LayerMask.NameToLayer("UI");
            var canvasComp = dialogCanvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            dialogCanvas.AddComponent<CanvasScaler>();
            dialogCanvas.AddComponent<GraphicRaycaster>();
        }
        return dialogCanvas;
    }

    /// <summary>移除界面</summary>
    public void PopNode(BaseDialog dialog)
    {
        // uiDialogues.Remove(dialog);
        PopDialogue(dialog);
    }

    public void ShowTost(string msg, float time = 2f)
    {
        _ = ShowTostAsync(msg, time);
    }
    public async UniTask ShowTostAsync(string msg, float time = 2f, bool isWait = false)
    {
        var prefab = Resources.Load<GameObject>(UIDefine.UIToast.GetDialogPath());
        var toast = GameObject.Instantiate(prefab, GetDlgParent().transform);
        var text = toast.GetComponentInChildren<TextMeshProUGUI>();
        text.text = msg;
        var canvansGroup = toast.GetComponent<CanvasGroup>();
        canvansGroup.alpha = 0;
        canvansGroup.DOFade(1, 0.1f).SetEase(Ease.OutSine);
        await UniTask.Delay((int)(time * 1000));
        await canvansGroup.DOFade(0, 0.1f).SetEase(Ease.InSine).AsyncWaitForCompletion();
        GameObject.Destroy(toast);
    }
}
