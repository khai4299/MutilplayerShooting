using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Look : MonoBehaviourPunCallbacks
{
    public Transform player;
    public Transform cam;
    public Transform weapon;
    public Transform weaponCam;
    public float xSensitivity;
    public float ySensitivity;
    public static bool cursorLocked = true;
    private Quaternion camCenter;
    public float maxAngle;
    void Start()
    {
        camCenter = cam.localRotation;
    }

    void Update()
    {
        if (Pause.isOn) return;
        if (!photonView.IsMine) return;
        SetY();
        SetX();
        weaponCam.rotation = cam.rotation;
    }
    void SetY()
    {
        float xRotate = Input.GetAxis("Mouse Y")*ySensitivity*Time.deltaTime;
        Quaternion adj = Quaternion.AngleAxis(xRotate, -Vector3.right);
        Quaternion delta = cam.localRotation * adj;
        if (Quaternion.Angle(camCenter,delta)<maxAngle)
        {
            cam.localRotation = delta;
            weapon.localRotation = delta;
        }
        weapon.rotation = cam.rotation;

    }
    void SetX()
    {
        float yRotate = Input.GetAxis("Mouse X") * xSensitivity * Time.deltaTime;
        Quaternion adj = Quaternion.AngleAxis(yRotate, Vector3.up);
        Quaternion delta = player.localRotation * adj;
        player.localRotation = delta;
    }
   
}
