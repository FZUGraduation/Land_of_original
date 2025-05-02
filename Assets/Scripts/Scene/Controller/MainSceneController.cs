
using UnityEngine;

public class MainSceneController : SceneController
{
    private static Vector3 lastPos = Vector3.zero;
    private GameObject character;

    void Awake()
    {
        FrameEvent.Instance.On(FrameEvent.BeforeSceneLoder, OnBeforeSceneLoder, this);
    }

    void Start()
    {
        WindowManager.Instance.ShowDialog(UIDefine.UIMainScene);
        var prefab = SaveSlotData.Instance.heroDatas[0].ConfigData.worldPrefab;
        character = Instantiate(prefab, lastPos, Quaternion.identity);
        FrameEvent.Instance.Emit(FrameEvent.CreateWorldPlayer, character);
    }

    private void OnBeforeSceneLoder()
    {
        if (character != null)
        {
            lastPos = character.transform.position;
        }
    }
}
