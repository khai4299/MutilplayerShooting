using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using Photon.Pun;
public class Sway : MonoBehaviourPunCallbacks
{
    private Quaternion origin_rotation;
    public bool isMine;
    // Start is called before the first frame update
    void Start()
    {
        origin_rotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Pause.isOn) return;
        float xRotate = Input.GetAxis("Mouse Y") ;
        float yRotate = Input.GetAxis("Mouse X") ;
        if(!isMine)
        {
            xRotate = 0;
            yRotate = 0;
        }
        Quaternion xadj = Quaternion.AngleAxis(-yRotate, Vector3.up);
        Quaternion yadj = Quaternion.AngleAxis(xRotate, Vector3.right);
        Quaternion target_rotation = origin_rotation * xadj * yadj;
        transform.localRotation = Quaternion.Lerp(transform.localRotation,target_rotation,Time.deltaTime*2);
    }
}
