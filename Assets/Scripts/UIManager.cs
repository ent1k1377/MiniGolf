using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System;
using UnityEngine.UI;

public class UIManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI RegLogin;
    [SerializeField] private TMP_InputField RegPassword;
    [SerializeField] private TMP_InputField RegRepeatPassword;
    [SerializeField] private TextMeshProUGUI RegError;

    [SerializeField] private TextMeshProUGUI Login;
    [SerializeField] private TMP_InputField Password;
    [SerializeField] private TextMeshProUGUI Error;

    [SerializeField] private GameObject StartCanvas;
    [SerializeField] private GameObject AutorizationCanvas;
    [SerializeField] private GameObject RegistrationCanvas;
    [SerializeField] private GameObject LoadingCanvas;

    private string selectText = "-";
    private UnityWebRequest request;
    private string url;
    private string text;

    static string GetHash(string plaintext)
    {
        var sha = new SHA1Managed();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(plaintext));
        return Convert.ToBase64String(hash);
    }
    public void SetActiveCanvas(bool f1=false, bool f2=false, bool f3=false)
    {
        StartCanvas.SetActive(f1);
        AutorizationCanvas.SetActive(f2);
        RegistrationCanvas.SetActive(f3);
    }
    private void Awake()
    {
        url = "http://minigolf.local/";
        SetActiveCanvas(f1:true);
        
    }

    private IEnumerator InsertQuery(string text)
    {
        WWWForm form = new WWWForm();
        form.AddField("text", text);
        UnityWebRequest request = UnityWebRequest.Post(this.url + "insertQuery.php", form);
        yield return request.SendWebRequest();
    }
    
    private IEnumerator SelectQuery(string text)
    {
        WWWForm form = new WWWForm();
        form.AddField("text", text);
        request = UnityWebRequest.Post(this.url + "selectQuery.php", form);
        yield return request.SendWebRequest();
        selectText = request.downloadHandler.text;
    }
    
    public void Registration()
    {
        StartCoroutine(RegValidationField());
    }
    private IEnumerator RegValidationField()
    {
        RegError.text = "";
        yield return StartCoroutine(SelectQuery(string.Format("users WHERE login = '{0}'", RegLogin.text)));
        print(selectText);
        if (selectText != "")
        {
            RegError.text = "Такой пользователь уже существует";
            yield break;
        }

        if (RegPassword.text.Length < 6)
        {
            RegError.text = "В пароле должно быть больше 6 символов";
            yield break;
        }

        if (RegPassword.text != RegRepeatPassword.text)
        {
            RegError.text = "Пароли не совпадают";
            yield break;
        }
        
        text = string.Format("users (login, password) VALUES('{0}', '{1}')", RegLogin.text, GetHash(RegPassword.text));
        StartCoroutine(InsertQuery(text));
        SetActiveCanvas(f2:true);
    }
    

    public void Autorization()
    {
        StartCoroutine(AutoValidationField());
    }

    private IEnumerator AutoValidationField()
    {
        print(GetHash(Password.text));
        Error.text = "";
        yield return StartCoroutine(SelectQuery(string.Format("users WHERE login = '{0}' AND password = '{1}'", Login.text, GetHash(Password.text))));
        if (selectText == "")
        {
            Error.text = "Логин или пароль не верны";
            yield break;
        }
        SetActiveCanvas();
        LoadingCanvas.SetActive(true);
        SetupSettings();
    }
    private void SetupSettings()
    {
        PhotonNetwork.NickName = Login.text;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Main");
    }
}
