using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Mirror;
using OVRSimpleJSON;
using UnityEngine;
using Random = UnityEngine.Random;
using XHFrameWork;

namespace FireCubeBase
{
    public class BaseNetManager : NetworkManager
    {
        /// <summary>
        /// 是否可以重置游戏
        /// </summary>
        public bool IsEnableResetGame { get; private set; }

        /// <summary>
        /// 偏移量
        /// </summary>
        public Vector3 ServerWorldOffest { get; set; }

        public override void Awake()
        {
            base.Awake();

#if !UNITY_ANDROID && !UNITY_EDITOR || UNITY_SERVER //服务端禁用ovrmanager
        // Destroy(OVRManager.instance);

         string jsonPath = Application.streamingAssetsPath + "/Configure.json";

        if (File.Exists(jsonPath))
        {
            //string json = File.ReadAllText(jsonPath);

            //JSONNode node = JSON.Parse(json);
            
            //Debug.Log($"加载的json文件：{json}  配置文件获取的IP地址：{node["SuperManager"].Value}");

            //SuperManagerIP = node["SuperManager"].Value;


        }
#endif


        }



        //-------------------------------------------------------------服务端-------------------------------------------------------------
        /// <summary>
        /// 单例网络管理器，服务端调用
        /// </summary>
        public static BaseNetManager FireCubeNetServer;

        private List<NetworkIdentity> _serverPlayerList = new List<NetworkIdentity>();

        /// <summary>
        /// 生成的网络物体，而且是非玩家的
        /// </summary>
        private Dictionary<string, NetworkIdentity> _serverObjectList = new Dictionary<string, NetworkIdentity>();

        /// <summary>
        /// 在服务端的场景sceneNet
        /// </summary>
        public SceneNet ScenenNet_Server;

        /// <summary>
        /// 一些流程中需要累计达到的步骤
        /// </summary>
        public Dictionary<string, int> CommDictionary = new Dictionary<string, int>();

        /// <summary>
        /// 客户端物体申请玩家权限的字典，key为客户端物体，value为玩家
        /// </summary>
        public Dictionary<NetworkIdentity, NetworkIdentity> AuthorityDic =
            new Dictionary<NetworkIdentity, NetworkIdentity>();


        public override void OnStartServer()
        {
            base.OnStartServer();



            NetworkServer.RegisterHandler<LogMessage>(OnLogMessage);
            //服务器一切静音
            AudioListener.volume = 0f;

            FireCubeNetServer = this;

            //服务器启动的时候就加载场景预制体

            GameObject rootScene = null;
            if (spawnPrefabs.Count > 0)
                rootScene = Instantiate(spawnPrefabs[0]);




            string jsonPath = Application.streamingAssetsPath + "/Configure.json";

            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);

                JSONNode node = JSON.Parse(json);

                ServerWorldOffest = new Vector3(node["localOffestX"].AsFloat, node["localOffestY"].AsFloat, node["localOffestZ"].AsFloat);

                Debug.Log($"node.value:{node.Value}  ServerOffest:{ServerWorldOffest}");

            }

            if (rootScene != null)
            {
                //服务器根场景位置归一化，后续会用它在客户端做参考
                rootScene.transform.position = new Vector3(0f, ServerWorldOffest.y, 0f);
                rootScene.transform.rotation = Quaternion.identity;


                //注册可能用到的消息


                NetworkServer.Spawn(rootScene);
            }
            else
            {
                Debug.LogError($"场景预制体为null");
            }




           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offest"></param>
        public void SetOffest(Vector3 localOffest, NetworkIdentity networkIdentity)
        {

            string jsonPath = Application.streamingAssetsPath + "/Configure.json";

            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);

                JSONNode node = JSON.Parse(json);



                node["localOffestX"].Value = localOffest.x.ToString();

                node["localOffestY"].Value = localOffest.y.ToString();

                node["localOffestZ"].Value = localOffest.z.ToString();

                Debug.Log($"node.value:{node.ToString()}");

                File.WriteAllText(jsonPath, node.ToString());


            }



            ServerWorldOffest = localOffest;

            //foreach (NetworkIdentity identity in _serverPlayerList)
            //{

            //    if (identity == networkIdentity) continue;

            //    identity.GetComponent<BaseNetPlayer>().RpcSetOffest(localOffest);
            //}
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            FireCubeNetServer = null;
            _serverObjectList.Clear();
            CommDictionary.Clear();
            _serverPlayerList.Clear();
            AuthorityDic.Clear();
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);

            Debug.LogError($"有客户端连接到服务器：{conn.identity} ip:{conn.address}  id:{conn.connectionId}");

            _serverPlayerList.Add(conn.identity);
        }

        /// <summary>
        /// 如果有客户端断开连接
        /// </summary>
        /// <param name="conn"></param>
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {

            if (_serverPlayerList.Contains(conn.identity))
            {
                _serverPlayerList.Remove(conn.identity);
            }
            base.OnServerDisconnect(conn);


        }

        /// <summary>
        /// 接收客户端的日记消息
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        public void OnLogMessage(NetworkConnectionToClient conn, LogMessage msg)
        {
            string message = msg.Log;

            string path = Application.streamingAssetsPath;

            string fileName = $"{conn.address}_{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}.txt";

            Thread t = new Thread(() =>
            {
                File.WriteAllText($"{path}/{fileName}", message);
            });
            t.Start();
        }

        /// <summary>
        /// 服务端生成网络物体
        /// </summary>
        /// <param name="prefabName"></param>
        public GameObject SpawnPrefab(string prefabName)
        {

            Debug.Log($"SpawnPrefab：{prefabName} ");


            if (!_serverObjectList.ContainsKey(prefabName))
            {
                foreach (GameObject prefab in spawnPrefabs)
                {
                    if (prefab.name == prefabName)
                    {
                        Debug.Log($"在服务端spawn物体：{prefabName}");
                        GameObject go = Instantiate(prefab);

                        BaseNet baseNet = go.GetComponent<BaseNet>();


                        if (prefabName.Contains("Name_UI"))//如果是UI  预制体的位置和旋转是摆放的时候的位置和旋转
                        {
                            if (baseNet.IsChangeHeight)
                            {
                                go.transform.localPosition = prefab.transform.position + new Vector3(0f, ServerWorldOffest.y, 0f);//用ServerWorldOffest.y的值认定添加的高度
                            }
                            else
                            {
                                go.transform.localPosition = prefab.transform.position;
                            }


                            go.transform.rotation = prefab.transform.rotation;

                            Debug.Log(prefab.transform.rotation.eulerAngles);

                        }
                        else  //这里就是场景物体了
                        {
                            go.transform.localPosition = new Vector3(0f, ServerWorldOffest.y, 0f);//用ServerWorldOffest.y的值认定添加的高度
                        }

                        NetworkServer.Spawn(go);

                        _serverObjectList.Add(prefabName, go.GetComponent<NetworkIdentity>());
                        return go;
                    }
                }


            }
            else
            {
                Debug.Log($"该物体已经生成过了，不能再次生成");
            }

            return null;
        }
        /// <summary>
        /// 广播给所有玩家，告诉服务器重启
        /// </summary>
        public void BroadcastResetGame()
        {
            foreach (NetworkIdentity identity in _serverPlayerList)
            {
                BaseNetPlayer baseNetPlayer = identity.GetComponent<BaseNetPlayer>();

                baseNetPlayer.RPCResetGame();
            }
        }
        public void UnSpawnGameObject(string objName)
        {
            if (_serverObjectList.ContainsKey(objName))
            {
                NetworkServer.UnSpawn(_serverObjectList[objName].gameObject);
            }
            else
            {
                Debug.LogError($"无法卸载掉该物体：{objName}");
            }

        }

        public void UnSpawnGameObjectServer(GameObject obj)
        {
            string key = null;
            foreach (KeyValuePair<string, NetworkIdentity> networkIdentity in _serverObjectList)
            {
                NetworkIdentity id = obj.GetComponent<NetworkIdentity>();

                if (id != null && id == networkIdentity.Value)
                {
                    NetworkServer.UnSpawn(obj.gameObject);
                    key = networkIdentity.Key;
                }
            }

            if (key != null)
            {
                _serverObjectList.Remove(key);
            }
        }



        //-------------------------------------------------------------客户端-------------------------------------------------------------


        /// <summary>
        /// 单例网络管理器，这个管理器在客户端调用方法
        /// </summary>
        public static BaseNetManager FireCubeNetClient;


        public List<NetworkIdentity> ClientPlayerList;

        public SceneNet ClientSceneNet;


        public void EnableResetGame()
        {
            IsEnableResetGame = true;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            FireCubeNetClient = this;

            NetworkClient.RegisterSpawnHandler(playerPrefab.GetComponent<NetworkIdentity>().assetId, Spawn, UnSpawn);

            foreach (GameObject o in spawnPrefabs)
            {
                NetworkClient.RegisterSpawnHandler(o.GetComponent<NetworkIdentity>().assetId, Spawn, UnSpawn);
            }

            //如果连接成功，该值改为默认值false
            IsEnableResetGame = false;

        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            FireCubeNetClient = null;


            NetworkClient.UnregisterSpawnHandler(playerPrefab.GetComponent<NetworkIdentity>().assetId);

            foreach (GameObject o in spawnPrefabs)
            {
                NetworkClient.UnregisterSpawnHandler(o.GetComponent<NetworkIdentity>().assetId);
            }
        }

        public virtual GameObject Spawn(SpawnMessage msg)
        {

            Debug.Log($"本地产生的物体的assetid:{msg.assetId} ");

            if (msg.assetId == playerPrefab.GetComponent<NetworkIdentity>().assetId)//加载player
            {
                GameObject go = Instantiate(playerPrefab, msg.position, msg.rotation);

                ClientPlayerList.Add(go.GetComponent<NetworkIdentity>());


                return go;
            }
            else
            {

                foreach (GameObject o in spawnPrefabs)//加载物体
                {
                    if (o.GetComponent<NetworkIdentity>().assetId == msg.assetId)
                    {
                        GameObject go = Instantiate(o, msg.position, msg.rotation);

                        ClientPlayerList.Add(go.GetComponent<NetworkIdentity>());



                        return go;
                    }
                }

            }



            return null;

        }
        public void UnSpawn(GameObject spawned)
        {
            BaseNetPlayer player = spawned.GetComponent<BaseNetPlayer>();
            if (player != null)
            {
                ClientPlayerList.Remove(player.netIdentity);
                Debug.Log($"移除客户端");
            }

            Destroy(spawned);
        }

        public override void OnClientError(TransportError error, string reason)
        {
            base.OnClientError(error, reason);

            Debug.Log($"OnClientError 连接错误：{error}");
        }

        public override void OnClientTransportException(Exception exception)
        {
            base.OnClientTransportException(exception);
            Debug.Log($"OnClientTransportException 连接错误：{exception.ToString()}");
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log($"OnClientDisconnect 客户端断开连接");
        }
    }

    /// <summary>
    /// 日记上传服务器消息
    /// </summary>
    public struct LogMessage : NetworkMessage
    {
        public string Log;
    }
}

