using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviourPunCallbacks
{
    public WeaponStat weapon;
    public float cooldown;
    public List<GameObject> target;
    public GameObject gunDisplay;
    private bool isDisable;
    private float wait;

    private void Start()
    {
        foreach (Transform item in gunDisplay.transform)
        {
            Destroy(item.gameObject);
        }
        GameObject newDisplay = Instantiate(weapon.disPlay, gunDisplay.transform.position, gunDisplay.transform.rotation);
        newDisplay.transform.SetParent(gunDisplay.transform);
    }
    private void Update()
    {
        if (isDisable)
        {
            if (wait>0)
            {
                wait -= Time.deltaTime;
            }
            else
            {
                Enable();
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == null) return;
        if (other.attachedRigidbody.gameObject.tag=="Player")
        {
            Weapon stat = other.attachedRigidbody.gameObject.GetComponent<Weapon>();
            Debug.Log(weapon.name);
            stat.photonView.RPC("PickupWeapon", RpcTarget.All, weapon.name);
            photonView.RPC("Disable", RpcTarget.All);
        }
    }
    [PunRPC]
    public void Disable()
    {
        isDisable = true;
        wait = cooldown;
        foreach (GameObject item in target)
        {
            item.SetActive(false);
        }
    }

    private void Enable()
    {
        isDisable = false;
        wait = 0;
        foreach (GameObject item in target)
        {
            item.SetActive(true);
        }
    }
}
