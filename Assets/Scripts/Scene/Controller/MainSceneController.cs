
using UnityEngine;
using UnityEngine.InputSystem;

public class MainSceneController : SceneController
{
    public static Vector3 lastPos = Vector3.zero;
    private GameObject character;
    private InputAction interactAction; // 定义一个 InputAction
    protected override void Awake()
    {
        base.Awake();
        FrameEvent.Instance.On(FrameEvent.BeforeSceneLoder, OnBeforeSceneLoder, this);
        FrameEvent.Instance.On(FrameEvent.ResetHeroBody, OnResetHeroBody, this);
        // 初始化 InputAction，绑定到 "ESC" 键
        interactAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/escape");
        interactAction.Enable(); // 启用 InputAction
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

    void Update()
    {
        if (interactAction.WasPressedThisFrame())
        {
            WindowManager.Instance.ShowDialog(UIDefine.UIPause);
        }
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
