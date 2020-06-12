using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Security.Cryptography;
using static HostGame;
using TMPro;

public class Motion : MonoBehaviourPunCallbacks,IPunObservable
{
    public float speed ;
    public float sprintMutifier;
    public float crouchMutifier;
    public float maxHeath;
    public float lengthofSlide;
    public float slideSpeed;
    public float jumpForce;
    private Rigidbody rigidbody;
    public GameObject cameraParent;
    public Camera playerCam;
    public Camera weaponCam;
    public GameObject weaponCamAim;
    public Transform weaponParent;
    private float defautView;
    private float sprintCamMutifier = 1.25f;
    private Vector3 orgin;
    public Transform groundCheck;
    public LayerMask ground;
    public float currentHeath;
    public Manager manager;
    public Image heathBar;
    public Image fuelFill;
    public Text username;
    public TextMeshProUGUI textName;
    [HideInInspector]
    public ProfileData playerProfile;

    private bool sliding;
    private float slide_time;
    private Vector3 slide_dir;

    public float slideAmount;
    public float crouchAmount;
    public GameObject stadingCollider;
    public GameObject crouchingCollider;
    private bool crouched;

    private Vector3 normalCamTarget;
    private Vector3 weaponCamTarget;
    private Vector3 weaponPosdefaut;
    private Vector3 weaponParentCurrentPos;
    private float aimAngle;

    private float movementCounter;
    private float idleCounter;
    private Vector3 targetWeaponBobPosition;
    bool crouch = false;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)(weaponParent.transform.localEulerAngles.x * 100f));
        }
        else
        {
            aimAngle =(int) stream.ReceiveNext()/100f;
        }
    }
    void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        currentHeath = maxHeath;     
        cameraParent.SetActive(photonView.IsMine);
        if (!photonView.IsMine)
        {
            gameObject.layer =LayerMask.NameToLayer("RemotePlayer");
            stadingCollider.layer = LayerMask.NameToLayer("RemotePlayer");
            crouchingCollider.layer = LayerMask.NameToLayer("RemotePlayer");
        }
        
        defautView = playerCam.fieldOfView;
        rigidbody = GetComponent<Rigidbody>();
        orgin = playerCam.transform.localPosition;
        weaponPosdefaut = weaponParent.localPosition;
        weaponParentCurrentPos = weaponPosdefaut;
        if (photonView.IsMine)
        {
            heathBar = GameObject.Find("HeathBar/Heath").GetComponent<Image>();
            fuelFill = GameObject.Find("Fuel/FuelFill").GetComponent<Image>();
            playerProfile = HostGame.myProfile;
            RefreshHealthBar();    
        }
    }

    bool pause = false;
    private void Update()
    {
        if (!photonView.IsMine)
        {
            //RefreshMultiplayerState();
            return;
        }
        if (Input.GetKey(KeyCode.K))
        {
            TakeDamage(100, -1);
        }
        username = GameObject.Find("Username/Text").GetComponent<Text>();
        
        photonView.RPC("SyncProfile", RpcTarget.All, playerProfile.name, playerProfile.level, playerProfile.xp);
        username.text = playerProfile.name;
        float xMov = Input.GetAxisRaw("Horizontal");
        float zMov = Input.GetAxisRaw("Vertical");


        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKey(KeyCode.Space);
        bool isGround = Physics.Raycast(groundCheck.position, Vector3.down, 0.5f, ground);
        bool isJump = jump && isGround;
        bool isSprinting = sprint && zMov > 0 && !isJump && isGround;
        if (Pause.isOn)
        {
            if (Cursor.lockState != CursorLockMode.None)
                Cursor.lockState = CursorLockMode.None;
            return;
        }
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (!isGround)
        {
            fuelFill.fillAmount -= 0.01f;
        }
        else
        {
            fuelFill.fillAmount += 0.005f;
        }
        RefreshHealthBar();
        //Head Bob
        if (!isGround)
        {
            //airborne
            HeadBob(idleCounter, 0.01f, 0.01f);
            idleCounter += 0;
            weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f * 0.2f);
        }
        else if (sliding)
        {
            //sliding
            HeadBob(movementCounter, 0.15f, 0.075f);
            weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f * 0.2f);
        }
        else if (xMov == 0 && zMov == 0)
        {
            //idling
            HeadBob(idleCounter, 0.01f, 0.01f);
            idleCounter += Time.deltaTime;
            weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f * 0.2f);
        }
        else if (!isSprinting && !crouched)
        {
            //walking
            HeadBob(movementCounter, 0.035f, 0.035f);
            movementCounter += Time.deltaTime * 6f;
            weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f * 0.2f);
        }
        else if (crouched)
        {
            //crouching
            HeadBob(movementCounter, 0.02f, 0.02f);
            movementCounter += Time.deltaTime * 4f;
            weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f * 0.2f);
        }
        else
        {
            //sprinting
            HeadBob(movementCounter, 0.15f, 0.055f);
            movementCounter += Time.deltaTime * 13.5f;
            weaponParent.localPosition = Vector3.MoveTowards(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f * 0.2f);
        }
        
    }
    void FixedUpdate()
    {
        if (Pause.isOn) return;
        if (!photonView.IsMine) return;
        float xMov = Input.GetAxisRaw("Horizontal");
        float zMov = Input.GetAxisRaw("Vertical");

        bool slide = Input.GetKey(KeyCode.LeftControl);
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKey(KeyCode.Space);
        float adjSpeed = speed;
       
        bool isGround = Physics.Raycast(groundCheck.position, Vector3.down, 0.5f, ground);
        bool isJump=jump &&isGround;    
        bool isSprinting = sprint && zMov > 0 && !isJump;
        bool isSliding = isSprinting && slide &&!sliding;
        if (Input.GetKeyDown(KeyCode.C))
        {
            crouch = !crouch;
        }
        bool isCrouching = crouch && !isSprinting && !isJump && isGround;

        if (isCrouching)
        {
            photonView.RPC("SetCrouch", RpcTarget.All, true);
            crouched = true;
        }
       else
        {
            photonView.RPC("SetCrouch", RpcTarget.All, false);
            crouched = false;
        }
        if (isJump && fuelFill.fillAmount>0)
        {
            rigidbody.AddForce(Vector3.up * jumpForce);
            if (crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);
            fuelFill.fillAmount -= 0.005f;
        }
        else
        {
            fuelFill.fillAmount += 0.004f;
        }

        
        Vector3 direction = Vector3.zero;
        if (!sliding)
        {
            direction = new Vector3(xMov, 0f, zMov);
            direction.Normalize();
            direction = transform.TransformDirection(direction);
            if (isSprinting)
            {
                if (crouched) photonView.RPC("SetCrouch", RpcTarget.All, false);
                adjSpeed *= sprintMutifier;
            }
            else if (crouched)
            {
                adjSpeed *= crouchMutifier;
            }
           
        }
        else
        {
            direction = slide_dir;
            adjSpeed *= slideSpeed;
            slide_time -= Time.deltaTime;
            if (slide_time <= 0)
            {
                sliding = false;
                weaponParentCurrentPos -= Vector3.down * (slideAmount - crouchAmount)*3f;
            }
        }
        Vector3 targetVeclocity = direction * adjSpeed * Time.fixedDeltaTime ;
        targetVeclocity.y = rigidbody.velocity.y;
        rigidbody.velocity = targetVeclocity;
        if (isSliding)
        {
            sliding = true;
            slide_dir = direction;
            slide_time = lengthofSlide;
            weaponParentCurrentPos += Vector3.down * (slideAmount - crouchAmount)*3f;
            if (!crouched) photonView.RPC("SetCrouch", RpcTarget.All, true);
        }
        if (sliding)
        {
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, defautView * sprintCamMutifier * 1.15f, Time.deltaTime * 8f);
            playerCam.transform.localPosition = Vector3.Lerp(playerCam.transform.localPosition, orgin + Vector3.down * 0.5f, Time.deltaTime * 5f);

            normalCamTarget = Vector3.MoveTowards(playerCam.transform.localPosition, orgin + Vector3.down * slideAmount, Time.deltaTime);
            weaponCamTarget = Vector3.MoveTowards(weaponCam.transform.localPosition, orgin + Vector3.down * slideAmount, Time.deltaTime);
        }
        else
        {
            if (isSprinting)
            {
                playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, defautView * sprintCamMutifier, Time.deltaTime * 8f);
                weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, defautView * sprintCamMutifier * 1.15f, Time.deltaTime * 8f);
            }
            else
            {
                playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, defautView, Time.deltaTime * 8f);
                weaponCam.fieldOfView = Mathf.Lerp(weaponCam.fieldOfView, defautView, Time.deltaTime * 8f);
            }
            if (crouched)
            {
                normalCamTarget = Vector3.MoveTowards(playerCam.transform.localPosition, orgin + Vector3.down * crouchAmount, Time.deltaTime);
                weaponCamTarget = Vector3.MoveTowards(weaponCam.transform.localPosition, orgin + Vector3.down * crouchAmount, Time.deltaTime);
            }
            else
            {
                normalCamTarget = Vector3.MoveTowards(playerCam.transform.localPosition, orgin, Time.deltaTime);
                weaponCamTarget = Vector3.MoveTowards(weaponCam.transform.localPosition, orgin , Time.deltaTime);
            }
        }
    }

    private void LateUpdate()
    {
        playerCam.transform.localPosition = normalCamTarget;
        weaponCam.transform.localPosition = weaponCamTarget;
    }
    public void TakeDamage(int amount, int actor)
    {
        if (photonView.IsMine)
        { 
            currentHeath -= amount;
            RefreshHealthBar();
            if (currentHeath <= 0)
            {
                StartCoroutine(TimetoSpawn());
                manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

                if (actor>=0)
                {
                    manager.ChangeStat_S(actor, 0, 1);
                }
                PhotonNetwork.Destroy(gameObject);
            }
        }
       
    }
    void RefreshHealthBar()
    {
        if (photonView.IsMine && this !=null)
        {
            heathBar.fillAmount = (currentHeath / maxHeath);
        }
    }
    private IEnumerator TimetoSpawn()
    {
        manager.Spawn();
        yield return new WaitForSeconds(2f);
    }
    [PunRPC]
    void SetCrouch(bool p_state)
    {
        if (crouched == p_state) return;

        crouched = p_state;

        if (crouched)
        {
            stadingCollider.SetActive(false);
            crouchingCollider.SetActive(true);
            weaponParentCurrentPos += Vector3.down * crouchAmount;
            crouched = true;
        }
        else
        {
            stadingCollider.SetActive(true);
            crouchingCollider.SetActive(false);
            weaponParentCurrentPos -= Vector3.down * crouchAmount;
            crouched = false;
        }
    }
    void RefreshMultiplayerState()
    {
        float cacheEulY = weaponParent.localEulerAngles.y;

        Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(aimAngle, Vector3.right);
        weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 8f);

        Vector3 finalRotation = weaponParent.localEulerAngles;
        finalRotation.y = cacheEulY;

        weaponParent.localEulerAngles = finalRotation;
    }
    [PunRPC]
    private void SyncProfile(string p_username,int p_level,int p_exp)
    {
        playerProfile = new ProfileData(p_username, p_level, p_exp);
        textName.text = playerProfile.name;
    }
    void HeadBob(float p_z, float p_x_intensity, float p_y_intensity)
    {
        targetWeaponBobPosition = weaponParentCurrentPos + new Vector3(Mathf.Cos(p_z) * p_x_intensity*1.5f, Mathf.Sin(p_z * 2) * p_y_intensity*1.5f , 0);
    }
    public void TrySync()
    {
        if (!photonView.IsMine) return;
        photonView.RPC("SyncProfile", RpcTarget.All, HostGame.myProfile.name, HostGame.myProfile.level, HostGame.myProfile.xp);
    }
}
