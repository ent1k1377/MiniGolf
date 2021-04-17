using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UserManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI RegLogin;
    [SerializeField] private TextMeshProUGUI RegPassword;
    [SerializeField] private TextMeshProUGUI RegRepeatPassword;
    public void Registration()
    {
        print(RegLogin.text);
        print(RegPassword.text);
        print(RegRepeatPassword.text);

    }
}
