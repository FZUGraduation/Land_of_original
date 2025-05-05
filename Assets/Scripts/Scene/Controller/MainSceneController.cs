
using UnityEngine;

public class MainSceneController : SceneController
{
    private static Vector3 lastPos = Vector3.zero;
    private GameObject character;

    protected override void Awake()
    {
        base.Awake();
        FrameEvent.Instance.On(FrameEvent.BeforeSceneLoder, OnBeforeSceneLoder, this);
        FrameEvent.Instance.On(FrameEvent.ResetHeroBody, OnResetHeroBody, this);
    }

    void Start()
    {
        WindowManager.Instance.ShowDialog(UIDefine.UIMainScene);
        var prefab = SaveSlotData.Instance.heroDatas[0].GetWorldPrefab();
        character = Instantiate(prefab, lastPos, Quaternion.identity);
        // characterModelController = character.GetComponentInChildren<CharacterModelController>();
        // characterModelController.SetWeapon(SaveSlotData.Instance.heroDatas[0].equipmentData[EquipmentType.Weapon]);
        FrameEvent.Instance.Emit(FrameEvent.CreateWorldPlayer, character);
    }

    private void OnBeforeSceneLoder()
    {
        if (character != null)
        {
            lastPos = character.transform.position;
        }
    }

    private void OnResetHeroBody()
    {
        if (character != null)
        {
            lastPos = character.transform.position;
            Destroy(character);
            character = null;
        }
        var prefab = SaveSlotData.Instance.heroDatas[0].GetWorldPrefab();
        character = Instantiate(prefab, lastPos, Quaternion.identity);
        FrameEvent.Instance.Emit(FrameEvent.CreateWorldPlayer, character);
    }

}
