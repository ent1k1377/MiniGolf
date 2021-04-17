using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class WaitingRoom : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI listPlayers;
    private void Start()
    {
        PhotonView photonView = GetComponent<PhotonView>();
        if (PhotonNetwork.IsMasterClient)
        {
            this.photonView.RPC("ShowPlayers", RpcTarget.All);
        }
        
    }
    private bool isFullRoom = false;
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && !isFullRoom)
        {
            print(444555);
            if (PhotonNetwork.PlayerList.Length == 2)
            {
                isFullRoom = true;
                PhotonNetwork.LoadLevel("Level 1");
            }
        }
    }
    
    [PunRPC]
    private void ShowPlayers()
    {
        print(333444);
        int countPlayers = 0;
        listPlayers.text = "";
        foreach (var player in PhotonNetwork.PlayerList)
        {
            countPlayers += 1;
            listPlayers.text += string.Format("{0}. {1}\n", countPlayers, player.NickName);
        }
        
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Main");
    }
    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        this.photonView.RPC("ShowPlayers", RpcTarget.All);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        this.photonView.RPC("ShowPlayers", RpcTarget.All);
    }
}
