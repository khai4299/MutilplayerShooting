using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using static HostGame;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInfo
{
    public ProfileData profileData;
    public int actor;
    public int kills;
    public int deaths;

    public PlayerInfo(ProfileData p,int a, int k,int d)
    {
        profileData = p;
        actor = a;
        kills = k;
        deaths = d;
    }
    public PlayerInfo()
    {
        profileData = null;
        actor = 0;
        kills = 0;
        deaths = 0;
    }
    
}
public enum GameState
{
    Waiting=0,
    Starting=1,
    Playing=2,
    Ending=3
}
public class Manager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public int mainmenu = 0;
    public int killcount = 3;
    public GameObject mapcam;
    public string playerPrefab_string;
    public GameObject playerPrefab;
    public GameObject[] spawnPos;
    public List<PlayerInfo> playerInfos = new List<PlayerInfo>();
    public int myind;

    public Text killCount;
    public Text deathCount;
    public GameObject leaderboardUI;
    public GameObject ui_endgame;
    private GameState state = GameState.Waiting;
    public enum EventCodes : byte
    {
        NewPlayer,
        UpdatePlayers,
        ChangeStat,
        NewMatch,
        RefreshTimer
    }
    // Start is called before the first frame update
    void Start()
    {
        mapcam.SetActive(false);
        ValidateConnection();
        killCount = GameObject.Find("Kill&Death/Kill").GetComponent<Text>();
        deathCount = GameObject.Find("Kill&Death/Death").GetComponent<Text>();
        NewPlayer_S(HostGame.myProfile);
        Spawn();
    }
    private void Update()
    {
        if (state == GameState.Ending)
            return;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (leaderboardUI.gameObject.activeSelf) leaderboardUI.gameObject.SetActive(!leaderboardUI.activeSelf);
            else Leaderboard(leaderboardUI);
        }
    }
    public void Spawn()
    {
        Transform spawn = spawnPos[Random.Range(0, spawnPos.Length)].transform;
        PhotonNetwork.Instantiate(playerPrefab_string, spawn.position, spawn.rotation);
    }

    private void Leaderboard(GameObject p_lb)
    {
        for (int i = 2; i < p_lb.transform.childCount; i++)
        {
            Destroy(p_lb.transform.GetChild(i).gameObject);
        }

        // cache prefab
        GameObject playercard = p_lb.transform.GetChild(1).gameObject;
        playercard.SetActive(false);

        // sort
        List<PlayerInfo> sorted = SortPlayers(playerInfos);

        // display
        bool t_alternateColors = false;
        foreach (PlayerInfo a in sorted)    
        {
            GameObject newcard = Instantiate(playercard, p_lb.transform) as GameObject;
            if (t_alternateColors) newcard.GetComponent<Image>().color = new Color32(0, 0, 0, 180);
            t_alternateColors = !t_alternateColors;

            newcard.transform.Find("Name").GetComponent<Text>().text = a.profileData.name;
            newcard.transform.Find("ScoreValue").GetComponent<Text>().text = (a.kills * 100).ToString();
            newcard.transform.Find("KillValue").GetComponent<Text>().text = a.kills.ToString();
            newcard.transform.Find("DeathValue").GetComponent<Text>().text = a.deaths.ToString();
            newcard.SetActive(true);
        }
        // activate
        p_lb.gameObject.SetActive(true);
        p_lb.transform.parent.gameObject.SetActive(true);
    }
    private List<PlayerInfo> SortPlayers(List<PlayerInfo> p_info)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();
            while (sorted.Count < p_info.Count)
            {
                // set defaults
                int highest = -1;
                PlayerInfo selection = p_info[0];

                // grab next highest player
                foreach (PlayerInfo a in p_info)
                {
                    if (sorted.Contains(a)) continue;
                    if (a.kills > highest)
                    {
                        selection = a;
                        highest = a.kills;
                    }
                }

                // add player
                sorted.Add(selection);
            }

        return sorted;
    }
    private void ValidateConnection()
    {
        if (PhotonNetwork.IsConnected) return;
        SceneManager.LoadScene(0);
    }
    private void RefreshMyStats()
    {
        if (playerInfos.Count > myind)
        {
            killCount.text = $"{playerInfos[myind].kills} kills";
            deathCount.text = $"{playerInfos[myind].deaths} deaths";
        }
        else
        {
            killCount.text = "0 kills";
            deathCount.text = "0 deaths";
        }
    }
    public void NewPlayer_S(ProfileData data)
    {
        //Thong tin khac nhau cua moi player khi tao
        object[] package = new object[6];
        package[0] = data.name;
        package[1] = data.level;
        package[2] = data.xp;
        package[3] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[4] = 0;
        package[5] = 0;
        //
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );
       
    }
    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    public void NewPlayer_R(object[] data)
    {

        PlayerInfo player = new PlayerInfo(
            new ProfileData(
            (string)data[0],
            (int)data[1],
            (int)data[2]),
            (int)data[3],
            (int)data[4],
            (int)data[5]
        );
        
        playerInfos.Add(player);
        UpdatePlayer_S(playerInfos);
    }
    public void UpdatePlayer_S(List<PlayerInfo> info)
    {
        object[] package = new object[info.Count];
        for (int i = 0; i < info.Count; i++)
        {
            object[] piece = new object[6];
            piece[0] = info[i].profileData.name;
            piece[1] = info[i].profileData.level;
            piece[2] = info[i].profileData.xp;
            piece[3] = info[i].actor;
            piece[4] = info[i].kills;
            piece[5] = info[i].deaths;

            package[i] = piece;
        }
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdatePlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }
    public void UpdatePlayer_R(object[] data)
    {

        playerInfos = new List<PlayerInfo>();
        Debug.Log(playerInfos.Count);
        Debug.Log(data.Length);
        for (int i = 0; i < data.Length; i++)
        {
            object[] extract = (object[])data[i];

            PlayerInfo p = new PlayerInfo(
                new ProfileData(
                    (string)extract[0],
                    (int)extract[1],
                    (int)extract[2]
                ),
                (int)extract[3],
                (int)extract[4],
                (int)extract[5]
            );

            playerInfos.Add(p);

            if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor)
            {
                myind = i ;
            }
        }

    }
    public void ChangeStat_S(int actor,byte stat,byte amt)
    {
        
        object[] pakage = new object[] { actor, stat, amt };
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ChangeStat,
            pakage,
            new RaiseEventOptions { Receivers=ReceiverGroup.All},
            new SendOptions { Reliability=true}
            );
    }
    public void ChangeStat_R(object[] data)
    {
        int actor = (int)data[0];
        byte stat = (byte)data[1];
        byte amt = (byte)data[2];
        for (int i = 0; i < playerInfos.Count; i++)
        {
            if(playerInfos[i].actor==actor)
            {
                switch(stat)
                {
                    case 0: //kills
                        playerInfos[i].kills += amt;
                        Debug.Log($"Player {playerInfos[i].profileData.name} : kills = {playerInfos[i].kills}");
                        break;

                    case 1: //deaths
                        playerInfos[i].deaths += amt;
                        Debug.Log($"Player {playerInfos[i].profileData.name} : deaths = {playerInfos[i].deaths}");
                        break;
                }
                Debug.Log(i + " "+myind);
                if (i == myind)
                {
                    RefreshMyStats();
                }
                if (leaderboardUI.gameObject.activeSelf) Leaderboard(leaderboardUI);
                break;
            }
        }
    }
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code >= 200) return;
        EventCodes eCode = (EventCodes)photonEvent.Code;
        object[] obj = (object[])photonEvent.CustomData;

        switch (eCode)
        {
            case EventCodes.ChangeStat:
                ChangeStat_R(obj);
                break;
            case EventCodes.NewPlayer:
                NewPlayer_R(obj);
                break;
            case EventCodes.UpdatePlayers:
                UpdatePlayer_R(obj);
                break;
        }
    }
}
