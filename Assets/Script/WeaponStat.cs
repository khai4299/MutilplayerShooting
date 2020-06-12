using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponStat 
{
    public string name;
    public int damage;
    public float range;
    public float fireRate;
    public float bloom;
    public int bullet;
    public GameObject Gunprefabs;
    public GameObject hitEffect;
    public GameObject disPlay;
    private int currentBullet;
    public void Initialize()
    {
        currentBullet = bullet; 
    }
    public bool FireToBullet()
    {
        if (currentBullet > 0)
        {
            currentBullet -= 1;
            return true;
        }
        else 
            return false;
    }
    public void Reload()
    {
        currentBullet = bullet;
    }
    public int GetBullet() { return currentBullet; }

}
