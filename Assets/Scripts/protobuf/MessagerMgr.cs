using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Google.Protobuf;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine;

namespace protobuf
{
    public class Constants
    {
        public const float SendDelay = 0.1f;
    }
    public class MessageMgr
    {
        public static TcpClient tcpClient;
        private ConcurrentQueue<FullMessage> messageQueue = new ConcurrentQueue<FullMessage>();

        public MessageMgr()
        {
            tcpClient = ConnectToServer();
            Thread receiveThread = new Thread(ReceiveMessage);
            Thread workerThread = new Thread(Worker);
            receiveThread.Start();
            workerThread.Start();

        }
        public TcpClient ConnectToServer()
        {
            TcpClient client = new TcpClient();
            try
            {
                client.Connect("192.168.163.128", 8080);
                Debug.Log("Connected to the server");
                return client;
            }
            catch (Exception e)
            {
                Debug.Log("Failed to connect to the server: " + e.Message);
                return null;
            }
        }

        private void ReceiveMessage()
        {
            while (true)
            {
                NetworkStream stream = tcpClient.GetStream();

                // 等待服务器返回数据，确保流上有可用数据
                if (stream.DataAvailable)
                {
                    // 读取数据长度
                    byte[] lengthBuffer = new byte[4];
                    stream.Read(lengthBuffer, 0, lengthBuffer.Length);
                    // 将数据长度转换为主机字节序
                    int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));

                    // 读取消息内容
                    byte[] buffer = new byte[length];
                    stream.Read(buffer, 0, buffer.Length);

                    try
                    {
                        // 使用 Parser.ParseFrom 从字节数组中解析消息
                        FullMessage message = FullMessage.Parser.ParseFrom(buffer);
                        messageQueue.Enqueue(message);
                        Debug.Log("Received message: " + message.Header.Type);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to parse message: " + e.Message);
                    }
                }


            }
        }

        private void Worker()
        {
            while (true)
            {
                if (messageQueue.TryDequeue(out FullMessage message))
                {
                    Debug.Log("worker , message type: " + message.Header.Type);
                    switch (message.Header.Type)
                    {
                        case MessageType.LoginInRsp:
                        {
                            LoginManager.Instance.OnLoginRsp(message);
                            break;
                        }
                        case MessageType.PlayerLogin:
                        {
                            WhenNewPlayerLogin(message);
                            break;
                        }
                        case MessageType.LoadOtherPlayers:
                        {
                            LoginManager.Instance.OnLoadOtherPlayers(message);
                            break;
                        }
                        case MessageType.PlayerState:
                        {
                            UpdateOthersPlayerState(message.PlayerState);
                            break;
                        } 
                        case MessageType.GunInfo:
                        {
                            WhenPlayerPickUpGun(message.GunInfo);
                            break;
                        }
                        case MessageType.GunFire:
                        {
                            WhenGunFire(message.GunFire);
                            break;
                        }
                        case MessageType.ReloadBullet:
                        {
                            WhenReloadBullet(message.ReloadBullet);
                            break;
                        }
                        case MessageType.BulletHit:
                        {
                            WhenBulletHit(message.BulletHit);
                            break;
                        }
                        case MessageType.AnimatorParam:
                        {
                            WhenAnimatorParam(message.AnimatorParam);
                            break;
                        }
                        case MessageType.RankList:
                        {
                            WhenRankList(message.RankList);
                            break;
                        }
                        default:
                            break;
                    }

                }
            }
        }
        

        private void WhenAnimatorParam(AnimatorParamMsg messageAnimatorParam)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GameObject gun = GameObject.Find("OtherPlayers_" + messageAnimatorParam.ClientId).
                    transform.Find("Assult_Rife_Arm").Find("Different_Weapons").Find(messageAnimatorParam.GunName).gameObject;
                if (gun == null)
                {
                    Debug.Log("Other player not found: " + messageAnimatorParam.ClientId);
                    return;
                }
                gun.GetComponent<OthersAutoGun>().WhenAnimatorParam(messageAnimatorParam);
            });
        }

        private void WhenPlayerPickUpGun(GunInfoMsg msg)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GameObject player = GameObject.Find("OtherPlayers_" + msg.ClientId);
                if (player == null)
                {
                    Debug.Log("Player not found");
                    return;
                }
                player.GetComponentInChildren<OthersGunsChange>(true).ChangeGun(msg.GunName,msg.Throw);
            });
        }

        private void WhenNewPlayerLogin(FullMessage message)
        {
            var otherPlayer = message.PlayerLogin.Client;
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                // 加载其他玩家预制体
                GameObject otherPlayersPrefab = Resources.Load<GameObject>("OtherPlayers");
                // 创建其他玩家对象
                var others =GameObject.Instantiate(otherPlayersPrefab, 
                    new Vector3(otherPlayer.Position.X, otherPlayer.Position.Y, otherPlayer.Position.Z), 
                    Quaternion.Euler(otherPlayer.Position.RotationX, otherPlayer.Position.RotationY, otherPlayer.Position.RotationZ));
                others.GetComponent<OtherPlayersControl>().Init(otherPlayer);
            });
        }
        
        private void WhenGunFire(GunFireMsg msg)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GameObject otherPlayer = GameObject.Find("OtherPlayers_" + msg.ClientId);
                if (otherPlayer == null)
                {
                    Debug.Log("Other player not found: " + msg.ClientId);
                    return;
                }
                otherPlayer.GetComponentInChildren<OthersAutoGun>(true).GunFire(msg);
            });
        }

        private void WhenReloadBullet(ReloadBulletMsg msg)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GameObject otherPlayer = GameObject.Find("OtherPlayers_" + msg.ClientId);
                if (otherPlayer == null)
                {
                    Debug.Log("Other player not found: " + msg.ClientId);
                    return;
                }
                otherPlayer.GetComponentInChildren<OthersAutoGun>(true).DoReloadAnimation(msg);
            });
        }

        private void WhenBulletHit(BulletHitMsg msg)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GameObject Player = GameObject.Find("OtherPlayers_" + msg.ClientId);
                if (Player == null)
                {
                    Debug.Log("Other player not found: " + msg.ClientId);
                    return;
                }
                Player.GetComponentInChildren<OtherPlayersControl>().BeBulletHit(msg);
            });
        }

        private void UpdateOthersPlayerState(PlayerStateMsg msg)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GameObject otherPlayer = GameObject.Find("OtherPlayers_" + msg.Client.ClientId);
                if (otherPlayer == null)
                {
                    Debug.Log("Other player not found: " + msg.Client.ClientId);
                    return;
                }
                otherPlayer.GetComponent<OtherPlayersControl>().UpdatePlayerPosition(msg.Client);
            });
        }
        
        private void WhenRankList(RankListMsg msg)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GameObject background = GameObject.Find("Aimlabs").transform.Find("BackGround").gameObject;
                if (background == null)
                {
                    Debug.Log("aimlabs background not found");
                    return;
                }
                background.GetComponent<RankList>().UpdateRankList(msg);
            });
        }
        

        public static bool SendMessageToServer(FullMessage message)
        {
            // 检查是否连接成功
            if (tcpClient == null || !tcpClient.Connected)
            {
                Debug.Log("Not connected to server.");
                return false;
            }
            // 将消息序列化为字节数组
            byte[] data = message.ToByteArray();

            // 将数据长度转换为网络字节序（大端序）
            int dataLength = IPAddress.HostToNetworkOrder(data.Length);
            byte[] lengthPrefix = BitConverter.GetBytes(dataLength);

            // 合并长度前缀和消息数据
            byte[] packet = new byte[lengthPrefix.Length + data.Length];
            Buffer.BlockCopy(lengthPrefix, 0, packet, 0, lengthPrefix.Length);
            Buffer.BlockCopy(data, 0, packet, lengthPrefix.Length, data.Length);

            try
            {
                // 获取网络流
                NetworkStream stream = tcpClient.GetStream();
                // 一次性发送完整消息包
                stream.Write(packet, 0, packet.Length);
                Debug.Log("Message sent to server :" + message);
                return true;
            }
            catch (Exception e)
            {
                Debug.Log("Failed to send message: " + e.Message);
                return false;
            }
        }
        
        
        
        
        
    }
}