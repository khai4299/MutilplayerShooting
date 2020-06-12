using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class GunLibrary : MonoBehaviour
{
    public WeaponStat[] stats;
    public static WeaponStat[] weapons;
    void Awake()
    {
        weapons = stats;
    }
    public static WeaponStat FindWeapon(string name)
    {
        foreach (WeaponStat item in weapons)
        {
            if (item.name.Equals(name))
            {
                Debug.Log("tim thay");
                return item;
            }
        }
        return weapons[0];
    }
}
