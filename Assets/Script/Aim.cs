using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aim : MonoBehaviourPunCallbacks
{
    private Animator animator;
    bool aim = false;
    public GameObject weaponCamAim;
    public GameObject playerCam;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        weaponCamAim.SetActive(false);
        
    }
    void Update()
    {
        if (Pause.isOn) return;
        if (!photonView.IsMine) return;
        playerCam = GameObject.Find("Cameras");
        if (Input.GetButtonDown("Fire2"))
        {
            Aiming();
            weaponCamAim.SetActive(true);
        }
        
    }
    public void Aiming()
    {
        aim = !aim;
        animator.SetBool("Aim", aim);
    }
}
