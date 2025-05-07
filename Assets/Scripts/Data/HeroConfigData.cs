using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class HeroConfigData : CharacterConfigData
{
    public EquipmentConfigData defaultWeapon;
    public EquipmentConfigData defaultBody;
    public bool isPlayer = false;
}
