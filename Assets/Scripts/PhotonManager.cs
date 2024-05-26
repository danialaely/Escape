using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField userNameText;
    public TMP_InputField roomNameText;
    public TMP_InputField maxPlayerText;

    public GameObject PlayerNamePanel;
    public GameObject LobbyPanel;
    public GameObject RoomCreatePanel;
    public GameObject ConnectingPanel;
    public GameObject RoomListPanel;

    private Dictionary<string, RoomInfo> roomListData;

    public GameObject roomListPrefab;
    public GameObject roomListParent;

    private Dictionary<string, GameObject> roomListGameobject;

    #region UnityMethods
    // Start is called before the first frame update
    void Start()
    {
        ActivateMyPanel(PlayerNamePanel.name);
        roomListData = new Dictionary<string, RoomInfo>();
        roomListGameobject = new Dictionary<string, GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Network State:"+PhotonNetwork.NetworkClientState);
    }
    #endregion

    #region UiMethods
    public void OnClick() 
    {
        string name = userNameText.text;
        if (!string.IsNullOrEmpty(name))
        {
            PhotonNetwork.LocalPlayer.NickName = name;
            PhotonNetwork.ConnectUsingSettings();
            ActivateMyPanel(ConnectingPanel.name);
        }
        else
        {
            Debug.Log("Empty Name");
        }
    }

    public void OnClickRoomCreate() 
    {
        string roomName = roomNameText.text;
        if (!string.IsNullOrEmpty(roomName)) 
        {
            roomName = roomName+ Random.Range(0, 1000);
        }
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte) int.Parse(maxPlayerText.text);
        PhotonNetwork.CreateRoom(roomName, roomOptions );  
    }

    public void OnCancelClick()
    {
        ActivateMyPanel(LobbyPanel.name);
    }

    public void RoomListBtnClicked() 
    {
        if (!PhotonNetwork.InLobby) 
        {
            PhotonNetwork.JoinLobby();
        }
        ActivateMyPanel(RoomListPanel.name);
    }

    public void BackFromRoomList() 
    {
        if (PhotonNetwork.InLobby) 
        {
            PhotonNetwork.LeaveLobby();
        }
    }

    #endregion

    #region Photon_Callbacks
    public override void OnConnected()
    {
        Debug.Log("Connected");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName+" is connected to photon.");
        ActivateMyPanel(LobbyPanel.name);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name+" is created!");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName+" Room Joined");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //Clear List
        ClearRoomList();

        foreach (RoomInfo rooms in roomList) 
        {
            Debug.Log("Room Name "+rooms.Name);
            if (!rooms.IsOpen || !rooms.IsVisible || rooms.RemovedFromList)
            {
                if (roomListData.ContainsKey(rooms.Name))
                {
                    roomListData.Remove(rooms.Name);
                }
            }
            else 
            {
                if (roomListData.ContainsKey(rooms.Name))
                {
                    roomListData[rooms.Name] = rooms;
                }
                else 
                {
                    roomListData.Add(rooms.Name,rooms);
                }
            }
        }

        //Generate List Item
        foreach (RoomInfo roomItem in roomListData.Values) 
        {
            GameObject roomListItemObject = Instantiate(roomListPrefab);
            roomListItemObject.transform.SetParent(roomListParent.transform);
            roomListItemObject.transform.localScale = Vector3.one;

            //romname  plaName JoinRoomBtn
            roomListItemObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = roomItem.Name;
            roomListItemObject.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = roomItem.PlayerCount+"/"+roomItem.MaxPlayers;
            roomListItemObject.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(() => RoomJoinFromList(roomItem.Name));
            roomListGameobject.Add(roomItem.Name, roomListItemObject);
        }
    }

    #endregion


    #region Public_Methods

    public void RoomJoinFromList(string roomName) 
    {
        if (PhotonNetwork.InLobby) 
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(roomName);
    }
    public void ClearRoomList() 
    {
        if (roomListGameobject.Count > 0)
        {
            foreach (var v in roomListGameobject.Values)
            {
                Destroy(v);
            }
            roomListGameobject.Clear();
        }
    }

    public void ActivateMyPanel(string panelName)
    {
        LobbyPanel.SetActive(panelName.Equals(LobbyPanel.name));
        PlayerNamePanel.SetActive(panelName.Equals(PlayerNamePanel.name));
        RoomCreatePanel.SetActive(panelName.Equals(RoomCreatePanel.name));
        ConnectingPanel.SetActive(panelName.Equals(ConnectingPanel.name));
        RoomListPanel.SetActive(panelName.Equals(RoomListPanel.name));
    }

    #endregion

}
