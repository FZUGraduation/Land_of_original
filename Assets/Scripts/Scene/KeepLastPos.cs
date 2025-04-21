
using UnityEngine;

public class KeepLastPos : MonoBehaviour
{
    public string guid;
    private void Awake()
    {
        FrameEvent.Instance.On(FrameEvent.BeforeSceneLoder, OnBeforeSceneLoder, this);

        //在C#代码中，直接对带有CharacterController组件的物体的transform.position赋值无法实现目标对象的瞬移
        if (SceneLoader.Instance.lastPosDict.ContainsKey(guid))
        {
            var character = GetComponent<CharacterController>();

            if (character != null) character.enabled = false;

            transform.position = SceneLoader.Instance.lastPosDict[guid];

            if (character != null) character.enabled = true;
        }
    }

    private void OnDestroy()
    {
        FrameEvent.Instance.OffAll(this);
    }
    private void OnBeforeSceneLoder()
    {
        SceneLoader.Instance.lastPosDict[guid] = transform.position;
    }
}
