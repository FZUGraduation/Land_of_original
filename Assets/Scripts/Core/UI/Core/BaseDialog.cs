
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class BaseDialog : MonoBehaviour
{
    // protected bool useMaskBg = true;
    [FoldoutGroup("BaseDialog"), Tooltip("点击背景关闭界面")]
    public bool clickMaskBgClose = false;
    [FoldoutGroup("BaseDialog"), Tooltip("遮罩背景")]
    public GameObject maskBg;
    [FoldoutGroup("BaseDialog"), Tooltip("是否可以多次打开")]
    public bool canMultiple = false;
    // public bool showPreDialog = false;//是否显示前一个界面
    [FoldoutGroup("BaseDialog"), Tooltip("关闭按钮")]
    public Button closeBtn;
    [FoldoutGroup("BaseDialog"), Tooltip("是否渐显")]
    public bool isFadeIn = true;
    // [FoldoutGroup("BaseDialog"), Tooltip("是否渐隐")]
    // public bool isFadeOut = true;

    protected virtual void Awake()
    {
        if (closeBtn)
        {
            closeBtn.onClick.AddListener(Close);
        }
        if (!maskBg)
        {
            maskBg = gameObject;
        }
        if (clickMaskBgClose)
        {
            var btn = maskBg.GetComponent<Button>();
            if (!btn)
            {
                btn = maskBg.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
            }
            if (btn)
            {
                btn.onClick.AddListener(Close);
            }
        }
    }
    protected virtual void OnEnable()
    {
        if (isFadeIn)
        {
            var canvansGroup = GetComponent<CanvasGroup>();
            if (!canvansGroup)
            {
                canvansGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvansGroup.DOFade(1, 0.3f).From(0);
        }
    }
    protected virtual void Start()
    {
        PlayDialogAnimation(true);
    }

    public virtual void Init(params object[] data) { }

    protected void Close()
    {
        OnCloseCallback();
    }
    private void OnCloseCallback()
    {
        OnClose();
        OnPopNode();
    }
    protected virtual void OnClose() { }

    private void OnPopNode()
    {
        WindowManager.Instance.PopNode(this);
    }

    /// <summary>播放窗口动画</summary>
    private bool PlayDialogAnimation(bool isShow)
    {
        var animation = GetComponent<Animation>();
        if (animation && gameObject.activeInHierarchy)
        {
            // 0:显示动画 1:隐藏动画
            int clipIndex = isShow ? 0 : 1;
            AnimationClip clip = GetAnimationClipByIndex(animation, clipIndex);
            if (clip != null)
            {
                animation.clip = clip;
                animation.Play();
                return true;
            }
        }
        return false;
    }
    private AnimationClip GetAnimationClipByIndex(Animation animation, int index)
    {
        int currentIndex = 0;
        foreach (AnimationState state in animation)
        {
            if (currentIndex == index)
            {
                return state.clip;
            }
            currentIndex++;
        }
        return null;
    }
    /// <summary>界面因为另一个界面的显示而需要隐藏时的处理,由windowmanager调用</summary>
    public virtual void OnBeHide()
    {
        gameObject.SetActive(false);
    }
    /// <summary>界面回到最上级时由windowmanager调用</summary>
    public virtual void OnBackToTop()
    {
        gameObject.SetActive(true);
    }
    /// <summary>界面再次显示调用的回调</summary>
    public virtual void OnResume()
    {
        PlayDialogAnimation(true);
    }
}
