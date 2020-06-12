using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Runtime.InteropServices.ComTypes;
using System.Linq.Expressions;

public class HostGame : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public class ProfileData
    {
        public string name;
        public int level;
        public int xp;
        public ProfileData()
        {
            name = "";
            level = 0;
            xp = 0;
        }
        public ProfileData(string n, int l, int x)
        {
            name = n;
            level = l;
            xp = x;
        }
    }
    public InputField username;
    public InputField roomnameField;
    public Slider maxplayerSlider;
    public Text maxplayerValue;
    public static ProfileData myProfile=new ProfileData();
    public GameObject tabMain;
    public GameObject tabRoom;
    public GameObject tabCreate;
    public GameObject buttonRoom;
    private List<RoomInfo> roomList;
    public void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        myProfile = Data.LoadProfile();
        Connect();
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        base.OnConnectedToMaster();
    }
    public override void OnJoinedRoom()
    {
        StartGame();
        base.OnJoinedRoom();
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
    }

    public void Connect()
    {
        PhotonNetwork.GameVersion = "0.0.0";
        PhotonNetwork.ConnectUsingSettings();
    }
    public void Join()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public void Create()
    {
        RoomOptions option = new RoomOptions();
        option.MaxPlayers = (byte) maxplayerSlider.value;
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
        properties.Add("map", 0);
        option.CustomRoomProperties = properties;
        PhotonNetwork.CreateRoom(roomnameField.text, option);
    }
    public void ChangeMap()
    {

    }
    public void ChangeMaxPlayerSlider(float value)
    {
        maxplayerValue.text = Mathf.RoundToInt(value).ToString();
    }
    public void TabCloseAll()
    {
        tabMain.SetActive(false);
        tabRoom.SetActive(false);
        tabCreate.SetActive(false);
    }
    public void TabOpenMain()
    {
        TabCloseAll();
        tabMain.SetActive(true);
    }
    public void TabOpenRoom()
    {
        TabCloseAll();
        tabRoom.SetActive(true);
    }
    public void TabOpenCreate()
    {
        TabCloseAll();
        tabCreate.SetActive(true);
    }
    private void ClearRoomList()
    {
        Transform content = tabRoom.transform.Find("Scroll View/Viewport/Content");
        foreach (Transform item in content) Destroy(item.gameObject);
    }
    public override void OnRoomListUpdate(List<RoomInfo> list)
    {
        roomList = list;
        ClearRoomList();
        Transform content = tabRoom.transform.Find("Scroll View/Viewport/Content");
        foreach (RoomInfo item in roomList)
        {
            GameObject newRoomButton = Instantiate(buttonRoom, content);
            newRoomButton.transform.Find("NameRoom").GetComponent<Text>().text = item.Name;
            newRoomButton.transform.Find("PlayerCount").GetComponent<Text>().text = item.PlayerCount + "/" + item.MaxPlayers;
            newRoomButton.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(newRoomButton.transform); });
        }
        base.OnRoomListUpdate(roomList);
    }
    public void JoinRoom(Transform button)
    {
        ApplyUsername();
        string roomName = button.Find("NameRoom").GetComponent<Text>().text;
        PhotonNetwork.JoinRoom(roomName); 
    }
    public void StartGame()
    {
        ApplyUsername();
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Data.SaveProfile(myProfile);
            PhotonNetwork.LoadLevel(1);
        }
    }
    private void ApplyUsername()
    {
        if (string.IsNullOrEmpty(username.text))
        {
            myProfile.name = "RANDOM_USER_" + Random.Range(100, 1000);
        }
        else
        {
            myProfile.name = username.text;
        }
    }
}