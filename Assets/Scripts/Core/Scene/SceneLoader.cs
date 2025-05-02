using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneLoader : Singleton<SceneLoader>
{
    #region 所有场景名字
    public const string startScene = "StartScene";  // 登录场景
    public const string mainScene = "MainScene";    // 主场景
    public const string loadingScene = "LoadingScene";  // 加载场景
    public const string battleScene = "BattleScene";  // 战斗场景
    #endregion

    public Dictionary<string, Vector3> lastPosDict = new();

    private Action onSceneLoaded = null;   // 场景加载完成回调

    private string nextSceneName = null;  // 将要加载的场景名
    private string currSceneName = null;   // 当前场景名，如若没有场景，则默认返回 Login
    public string CurrSceneName  //获取当前场景名
    {
        get
        {
            if (string.IsNullOrEmpty(Instance.currSceneName))
            {
                Instance.currSceneName = SceneManager.GetActiveScene().name;
            }
            return Instance.currSceneName;
        }
    }
    private string preSceneName = null;   // 上一个场景名

    private bool isOnLoading = false;     // 是否正在加载中

    private bool destroyAuto = true;  // 自动删除 loading 背景

    private GameObject loadingProgress = null;               // 加载进度显示对象
    private GameObject slider = null;                     // 加载进度条
    private SceneController sceneController = null;  // 场景控制器
    public SceneController SceneController
    {
        get
        {
            if (null == sceneController)
            {
                sceneController = GameObject.FindObjectOfType<SceneController>();
            }
            return sceneController;
        }
    }

    public Scene GetActiveScene()
    {
        return SceneManager.GetActiveScene();
    }

    /// <summary> 加载上一个场景 </summary>
    public void LoadPreScene()
    {
        if (string.IsNullOrEmpty(Instance.preSceneName))
        {
            return;
        }
        LoadScene(Instance.preSceneName);
    }
    /// <summary> 加载场景 </summary>
    public void LoadScene(string sceneName)
    {
        Instance.LoadLevel(sceneName, null);
    }
    /// <summary> 加载场景+回调 </summary>
    public void LoadScene(string sceneName, Action onSecenLoaded)
    {
        Instance.LoadLevel(sceneName, onSecenLoaded);
    }
    /// <summary> 加载Loading场景+回调+是否自动销毁 </summary>
    private void LoadLevel(string sceneName, Action onSecenLoaded, bool isDestroyAuto = true)
    {
        if (isOnLoading || currSceneName == sceneName)
        {
            return;
        }
        DOTween.KillAll();
        if (string.IsNullOrEmpty(currSceneName))
        {
            currSceneName = SceneManager.GetActiveScene().name;
        }
        FrameEvent.Instance.Emit(FrameEvent.BeforeSceneLoder, sceneName);
        isOnLoading = true;  // 开始加载  
        onSceneLoaded = onSecenLoaded;
        nextSceneName = sceneName;
        preSceneName = currSceneName;
        currSceneName = loadingScene;
        destroyAuto = isDestroyAuto;

        //先异步加载 Loading 界面
        _ = StartLoadSceneOnEditor(loadingScene, OnLoadingSceneLoaded, null);
    }


    /// <summary>过渡loading场景加载完成回调</summary>
    private void OnLoadingSceneLoaded()
    {
        // 过渡场景加载完成后加载下一个场景
        _ = StartLoadSceneOnEditor(nextSceneName, OnNextSceneLoaded, OnNextSceneProgress);
    }

    /// <summary>开始加载某个场景 + 结束后回调 + 进度变化回调</summary>
    async UniTask StartLoadSceneOnEditor(string loadSceneName, Action OnSecenLoaded, Action<float> OnSceneProgress)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(loadSceneName);
        if (null == async)
        {
            return;
        }

        //*加载进度
        while (!async.isDone)
        {
            float fProgressValue;
            if (async.progress < 0.9f)
            {
                fProgressValue = async.progress;
            }
            else
            {
                fProgressValue = 1.0f;
            }
            OnSceneProgress?.Invoke(fProgressValue);
            await UniTask.Yield();
        }

        Debug.Log("加载场景完成：" + loadSceneName);
        OnSecenLoaded?.Invoke();
    }

    /// <summary>加载下一场景完成回调</summary>
    private void OnNextSceneLoaded()
    {
        isOnLoading = false;
        OnNextSceneProgress(1);
        currSceneName = nextSceneName;
        nextSceneName = null;
        onSceneLoaded?.Invoke();
        FrameEvent.Instance.Emit(FrameEvent.AfterSceneLoder, currSceneName);
    }

    /// <summary>场景加载进度变化</summary>
    private void OnNextSceneProgress(float progress)
    {
        if (null == loadingProgress)
        {
            loadingProgress = GameObject.Find("TextLoadProgress");
            slider = GameObject.Find("Slider");
        }
        TextMeshProUGUI textLoadProgress = loadingProgress?.GetComponent<TextMeshProUGUI>();
        Slider sliderComponent = slider?.GetComponent<Slider>();
        if (null == textLoadProgress)
        {
            return;
        }
        textLoadProgress.text = (progress * 100).ToString() + "%";
        sliderComponent.value = progress;
    }
}
