using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Photon.Pun;
public class Weapon : MonoBehaviourPunCallbacks
{
    public List< WeaponStat> gun;
    public Transform gunParent;
    private GameObject currentWeapon;
    public int indexWeapon=0;
    public Camera cam;
    public LayerMask mask;
    public GameObject crosshairUI;
    public GameObject crosshairUiIns;
    float timeTOFire = 0;
    private bool isRealoading=false;
    // Start is called before the first frame update
    void Start()
    {
        foreach (WeaponStat wP in gun)
        {
            wP.Initialize();
        }
        Equip(indexWeapon);
    }
    // Update is called once per frame
    void Update()
    {
       
        if (Pause.isOn) return;
        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1))
        {

            currentWeapon.GetComponent<Animator>().SetBool("Equip", true);
            photonView.RPC("Equip", RpcTarget.All, 0);         
        }
        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentWeapon.GetComponent<Animator>().SetBool("Equip", true);
            photonView.RPC("Equip", RpcTarget.All, 1);
        }
        currentWeapon.GetComponent<Animator>().SetBool("Equip", false);
        if (gun[indexWeapon].GetBullet()<=0)
        {
            StartCoroutine(Reload());
            return;
        }
        if (currentWeapon!=null)
        {
            if (photonView.IsMine)
            {
                if (gun[indexWeapon].fireRate <= 0f && !isRealoading)
                {
                    if (Input.GetButton("Fire1") )
                    {
                        if (gun[indexWeapon].FireToBullet())
                            photonView.RPC("Shoot", RpcTarget.All);   
                    }
                }
                else
                {
                    if (Input.GetButton("Fire1") && Time.time > timeTOFire &&!isRealoading)
                    {
                        if (gun[indexWeapon].FireToBullet())
                        {
                            photonView.RPC("Shoot", RpcTarget.All);
                        }
                        timeTOFire = Time.time + 1 / gun[indexWeapon].fireRate;
                    }
                }
                if (Input.GetKeyDown(KeyCode.R)) photonView.RPC("ReloadRPC", RpcTarget.All);

            }
            currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
        }
    }
    [PunRPC]
    void Equip(int index)
    {

        if (currentWeapon != null)
        {
            if(isRealoading) StopCoroutine(Reload());
            Destroy(currentWeapon);
        }
        crosshairUiIns = GameObject.FindGameObjectWithTag("Crosshair");
        if (crosshairUiIns != null)
        {
            Destroy(crosshairUiIns);
        }
        indexWeapon = index;
        GameObject newWeapon = Instantiate(gun[index].Gunprefabs, gunParent.position, gunParent.rotation, gunParent);
        crosshairUiIns = Instantiate(crosshairUI);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localEulerAngles = Vector3.zero;
        newWeapon.GetComponent<Sway>().isMine = photonView.IsMine;
        currentWeapon = newWeapon;
    }
    [PunRPC]
    void PickupWeapon(string name)
    {
        WeaponStat weapon = GunLibrary.FindWeapon(name);
        weapon.Initialize();
        if (gun.Count >= 2)
        {
            Debug.Log("replace");
            gun[indexWeapon] = weapon;
            Equip(indexWeapon);
        }
        else
        {
            Debug.Log("new");
            gun.Add(weapon);
            Equip(gun.Count-1);
        }
        
    }
    [PunRPC]
    void Shoot()
    {
        Vector3 bloom = cam.transform.position + cam.transform.forward * 1000f;
        //bloom

            bloom += Random.Range(-20f, 20f) * cam.transform.up;
            bloom += Random.Range(-20f, 20f) * cam.transform.right;
            bloom -= cam.transform.position;
            bloom.Normalize();
      
        //Hit something
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, bloom, out hit, gun[indexWeapon].range, mask))
        {
            GameObject hitEffect = (GameObject)Instantiate(gun[indexWeapon].hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(hitEffect, 2f);
            if (photonView.IsMine)
            {
                if ( hit.collider.gameObject.layer == LayerMask.NameToLayer("RemotePlayer"))
                {
                   hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, gun[indexWeapon].damage,PhotonNetwork.LocalPlayer.ActorNumber);
                }
            }
        }
        //bloom     
        currentWeapon.transform.Rotate(gun[indexWeapon].bloom, 0, 0);
        cam.transform.Rotate(gun[indexWeapon].bloom, 0, 0);
        currentWeapon.transform.position -= currentWeapon.transform.forward * 0.1f;
        cam.transform.position -= cam.transform.forward * 0.1f;

    }
    [PunRPC]
    private void TakeDamage(int amount,int actor)
    {
        GetComponent<Motion>().TakeDamage(amount,actor);
    }

    [PunRPC]
    private void ReloadRPC()
    {
        StartCoroutine(Reload());
    }
    IEnumerator Reload()
    {
        isRealoading = true;
        currentWeapon.GetComponent<Animator>().SetBool("Reload", isRealoading);
        gun[indexWeapon].Reload();
        yield return new WaitForSeconds(1.617f);
        isRealoading = false;
        currentWeapon.GetComponent<Animator>().SetBool("Reload", isRealoading);
    }
    
}
