using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    public GameObject PlayerPrefab;
    public int hitHole = 0;
    public GameObject CanvasWin;
    public GameObject CanvasDefault;
    
    private float time = 0;
    private float timePassage = 55;
    private TextMeshProUGUI timeText;
    public float[,] levels = new float[3, 3];
    private string url = "http://minigolf.local/";
    private int place = 0;


    [PunRPC]
    public void UpdatePlace(string name)
    {
        place += 1;
    }
    public IEnumerator UpdateCoins(string name)
    {
        if (PhotonNetwork.NickName == name)
        {
            WWWForm form = new WWWForm();
            form.AddField("name", name);
            form.AddField("place", place);
            UnityWebRequest request = UnityWebRequest.Post(this.url + "updateCoins.php", form);
            yield return request.SendWebRequest();
            CanvasWin.SetActive(true);
            CanvasDefault.SetActive(false);
            
            GameObject.Find("text_win").GetComponent<TextMeshProUGUI>().text = string.Format("вы заняли\n\n {0} место", place);
            yield return new WaitForSeconds(1.5f);
            PhotonNetwork.LeaveRoom();
        }
    }
    private IEnumerator SelectInfoLevels()
    {
        WWWForm form = new WWWForm();
        UnityWebRequest request = UnityWebRequest.Post(this.url + "selectInfoLevels.php", form);
        yield return request.SendWebRequest();
        
        string[] array = request.downloadHandler.text.Replace("\n", "").Split(new char[] { '|' });

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                levels[i, j] = float.Parse(array[i].Replace(".", ",").Split(new char[] { ';' })[j]);
            }
        }
        InitializeManager();
        this.photonView.RPC("LevelInsert", RpcTarget.Others, (string[]) array);
        

    }
    
    [PunRPC]
    private void LevelInsert(string[] array)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                levels[i, j] = float.Parse(array[i].Replace(".", ",").Split(new char[] { ';' })[j]);
            }
        }
        InitializeManager();
    }
    private string id_match;
    private IEnumerator addUserMatches(int flag, int idMatch)
    {
        WWWForm form = new WWWForm();
        form.AddField("name", PhotonNetwork.NickName);
        form.AddField("flag", flag);
        form.AddField("idMatch", idMatch);
        UnityWebRequest request = UnityWebRequest.Post(this.url + "addUserMatch.php", form);
        yield return request.SendWebRequest();
        id_match = request.downloadHandler.text.Replace(" ", "");
        print(id_match);
        if (flag == 1)
        {
            this.photonView.RPC("addOtherUser", RpcTarget.Others, 2, int.Parse(id_match));
        }
        

    }

    [PunRPC]
    private void addOtherUser(int flag, int idMatch)
    {
        StartCoroutine(addUserMatches(flag, idMatch));
    }

    private void Start()
    {
        PhotonView photonView = GetComponent<PhotonView>();
        
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SelectInfoLevels());
            StartCoroutine(addUserMatches(1, 1));
        }
        if (!instance) 
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
    }
    private void InitializeManager()
    {
        
        //timeText = GameObject.Find("TimeText").GetComponent<TextMeshProUGUI>();
        
        Vector3 spawn_ball = new Vector3(levels[0, 0], levels[0, 1], levels[0, 2]);
        GameObject GO = PhotonNetwork.Instantiate(PlayerPrefab.name, spawn_ball, Quaternion.identity);
        GO.GetComponentInChildren<BallController>().SetupCamera(Camera.main);
    }


    //private void Update()
    //{
    //    Timer();
    //}


    //private void Timer()
    //{
    //    time += Time.deltaTime;
    //    if (timePassage - Math.Round(time) <= 10)
    //        timeText.text = (timePassage - Math.Round(time)).ToString();

    //    if (time >= timePassage) { }
    //        //Leave();
    //}

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
        Debug.LogFormat("{0} entered room", newPlayer.NickName);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat("{0} left room", otherPlayer.NickName);
    }
}
