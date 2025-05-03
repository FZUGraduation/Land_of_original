using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterModelController : MonoBehaviour
{
    public Transform root;
    public Transform weaponRoot;
    public void SetWeapon(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }
        var data = Datalib.Instance.GetData<EquipmentConfigData>(key);
        if (data == null)
        {
            return;
        }
        SetWeapon(data);
    }
    public void SetWeapon(EquipmentConfigData data)
    {
        if (data == null)
        {
            return;
        }
        for (int i = 0; i < weaponRoot.childCount; i++)
        {
            Destroy(weaponRoot.GetChild(i).gameObject);
        }
        GameObject go = Instantiate(data.prefab, weaponRoot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }
}
