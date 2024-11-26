using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using PimDeWitte.UnityMainThreadDispatcher;
using protobuf;
using TMPro;

// 单例模板类 ，继承自 MonoBehaviour
public class Singleton<T> : MonoBehaviour where T : new()
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }
            return instance;
        }
    }
}
public class LoginManager : Singleton<LoginManager>
{
    public TMP_InputField username;
    public TMP_InputField password;
    public Button loginButton;

    private bool loginSuccess = false;

    public Camera LoginCamera;
    public GameObject Login;
    void Start()
    {
        LoginCamera.gameObject.SetActive(true);
        Login.SetActive(true);
        
        MessageMgr messageMgr = new MessageMgr();
        loginButton.onClick.AddListener(OnLoginButtonClick);
        // 获取 Player 对象并禁用
        var player = GameObject.Find("PlayerParent").transform.Find("Player").gameObject;
        player.SetActive(false);
        Cursor.lockState=CursorLockMode.None;//解锁鼠标
    }
    

    private void OnLoginButtonClick()
    {
        if (!SendLoginReq())
        {
            Debug.Log("Failed to send login request.");
        }
    }

    private bool SendLoginReq()
    {
        string usernameText = username.text;
        string passwordText = password.text;

        if (string.IsNullOrEmpty(usernameText) || string.IsNullOrEmpty(passwordText))
        {
            Debug.Log("Username or password cannot be empty");
            return false;
        }

        // 创建 LoginInRequest 消息并设置字段
        var message=new FullMessage
        {
            Header = new MessageHeader
            {
                Type = MessageType.LoginInReq
            },
            LoginReq = new LoginInRequest
            {
                Username = usernameText,
                Password = passwordText
            }
        };
        // 发送消息
        return MessageMgr.SendMessageToServer(message);

    }


    public void OnLoginRsp(FullMessage message)
    {
        if (message.Header.Type == MessageType.LoginInRsp)
        {
            if (message.LoginRsp.ErrorNo == (int)LoginStatus.SUCCESS
                || message.LoginRsp.ErrorNo == (int)LoginStatus.NEW_USER)
            {
                var clientMsg = message.LoginRsp.Client;
                Debug.Log(clientMsg);

                ManualResetEvent resetEvent = new ManualResetEvent(false);
                // 主线程中执行激活操作,worker线程不能使用很多主线程才能使用的方法
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    var player = GameObject.Find("PlayerParent").transform.Find("Player").gameObject;
                    player.SetActive(true);
                    player.GetComponent<PlayerControl>().Init(clientMsg);
                    Cursor.lockState=CursorLockMode.Locked;//锁定鼠标
                    GameObject.Find("Login").transform.Find("Camera_UI").gameObject.SetActive(false);
                    GameObject.Find("Login").transform.Find("login").gameObject.SetActive(false);

                    // 完成后，发出信号通知非主线程
                    resetEvent.Set();
                });
                
                // 等待主线程完成信号
                resetEvent.WaitOne();
                
                loginSuccess = true;
                Debug.Log("Login success");
            }
            else
            {
                loginSuccess = false;
                Debug.Log("Login failed: " +(LoginStatus)message.LoginRsp.ErrorNo);
            }
        }
        else
        {
            Debug.Log("Invalid message type: " + message.Header.Type);
        }
    }
    
    public void OnLoadOtherPlayers(FullMessage message)
    {
        var otherPlayers = message.LoadOtherPlayers.OtherClients;
        foreach (var otherPlayer in otherPlayers)
        {
            Debug.Log("Other player: " + otherPlayer);
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            // 主线程中执行激活操作,worker线程不能使用很多主线程才能使用的方法
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                // 加载其他玩家预制体
                GameObject otherPlayersPrefab = Resources.Load<GameObject>("OtherPlayers");
                // 创建其他玩家对象
                var others =Instantiate(otherPlayersPrefab, 
                    new Vector3(otherPlayer.Position.X, otherPlayer.Position.Y, otherPlayer.Position.Z), 
                    Quaternion.Euler(otherPlayer.Position.RotationX, otherPlayer.Position.RotationY, otherPlayer.Position.RotationZ));
                others.GetComponent<OtherPlayersControl>().Init(otherPlayer);
                
                // 完成后，发出信号通知非主线程
                resetEvent.Set();
                Debug.Log(string.Format("Load other player {0} success",otherPlayer.Username));
            });
                
            // 等待主线程完成信号
            resetEvent.WaitOne();
        }
    }


    private void OnApplicationQuit()
    {
        if (loginSuccess)
        {
            var message = new FullMessage
            {
                Header = new MessageHeader
                {
                    Type = MessageType.Logout
                },
                Logout = new LogoutMsg()
                {
                    ClientId = GameObject.Find("PlayerParent").transform.Find("Player").gameObject.GetComponent<PlayerControl>().clientId,
                }
            };
            MessageMgr.SendMessageToServer(message);
        }
    }
}


public enum LoginStatus {
    SUCCESS = 0,
    PASSWORD_INCORRECT = 1,
    NEW_USER = 2,
    SQL_ERROR =3,
};
