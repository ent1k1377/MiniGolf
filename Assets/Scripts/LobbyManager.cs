using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private string url = "http://minigolf.local/";
    public TextMeshProUGUI LBText;
    public TextMeshProUGUI LUText;
    public TextMeshProUGUI WLText;
    private void Start()
    {
        StartCoroutine(SelectUserCoins());
        
    }
    public void CreateRoom()
    {
        EnterRoomParams roomParams = new EnterRoomParams();
        roomParams.RoomOptions = new RoomOptions { MaxPlayers = 6, CleanupCacheOnLeave = false };

        PhotonNetwork.NetworkingClient.OpJoinRandomOrCreateRoom(null, roomParams);
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined the room");
        PhotonNetwork.LoadLevel("Waiting Room");
        //PhotonNetwork.LoadLevel("GameTest");
    }
    public void Quit()
    {
        Application.Quit();
    }
    private IEnumerator SelectUserCoins()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", PhotonNetwork.NickName);
        UnityWebRequest request = UnityWebRequest.Post(this.url + "selectUserCoins.php", form);
        yield return request.SendWebRequest();
        string coins = request.downloadHandler.text.Replace(" ", "");
        GameObject.Find("coin_text").GetComponent<TextMeshProUGUI>().text = coins;
    }
    public void callSelectLeaderBoard()
    {
        StartCoroutine(SelectLeaderBoard());
    }
    private IEnumerator SelectLeaderBoard()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", PhotonNetwork.NickName);
        UnityWebRequest request = UnityWebRequest.Post(this.url + "leaderBoard.php", form);
        yield return request.SendWebRequest();
        string[] users = request.downloadHandler.text.Replace(" ", "").Split(new char[] { '\n' });
        LBText.text = "";
        for (int i = 0; i < users.Length - 1; i++)
        {
            LBText.text += string.Format("{0}. {1}\n", i + 1, users[i]);
        }
        
    }
    public void callStatistics()
    {
        StartCoroutine(SelectLastUsers());
        StartCoroutine(winLoseUser());
    }
    private IEnumerator SelectLastUsers()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", PhotonNetwork.NickName);
        UnityWebRequest request = UnityWebRequest.Post(this.url + "lastUsers.php", form);
        yield return request.SendWebRequest();
        string[] users = request.downloadHandler.text.Replace(" ", "").Split(new char[] { '\n' });
        LUText.text = "";
        for (int i = 0; i < users.Length - 1; i++)
        {
            LUText.text += string.Format("{0}. {1}\n", i + 1, users[i]);
        }

    }
    private IEnumerator winLoseUser()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", PhotonNetwork.NickName);
        UnityWebRequest request = UnityWebRequest.Post(this.url + "winLose.php", form);
        yield return request.SendWebRequest();
        string[] wl = request.downloadHandler.text.Replace(" ", "").Split(new char[] { '|' });
        print(wl[0]);
        WLText.text = string.Format("win: {0}   lose: {1}", wl[0], wl[1]);
        

    }
}
