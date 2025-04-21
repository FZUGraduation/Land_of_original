using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestScript1 : MonoBehaviour
{
    // private Renderer lastRenderer;
    // private Color originalColor;
    // private Camera mainCamera;
    // private void Start()
    // {
    //     mainCamera = Camera.main;
    // }

    // private void Update()
    // {
    //     if (IsMouseOverObject(out RaycastHit hit))
    //     {
    //         Renderer renderer = hit.collider.gameObject.GetComponent<Renderer>();
    //         if (renderer != null)
    //         {
    //             if (lastRenderer != null && lastRenderer != renderer)
    //             {
    //                 lastRenderer.material.color = originalColor;
    //             }
    //             if (lastRenderer == renderer)
    //             {
    //                 return;
    //             }

    //             originalColor = renderer.material.color;
    //             renderer.material.color = Color.yellow;
    //             lastRenderer = renderer;
    //         }
    //     }
    //     else if (lastRenderer != null)
    //     {
    //         lastRenderer.material.color = originalColor;
    //         lastRenderer = null;
    //     }
    // }
    // private bool IsMouseOverObject(out RaycastHit hit)
    // {
    //     // 检查鼠标是否在 UI 组件上
    //     if (EventSystem.current.IsPointerOverGameObject())
    //     {
    //         hit = default;
    //         return false;
    //     }

    //     Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    //     return Physics.Raycast(ray, out hit);
    // }

}
