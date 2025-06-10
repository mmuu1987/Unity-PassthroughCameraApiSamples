using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using Mirror;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace FireCubeBase
{
    /// <summary>
    /// 本地管理器，单例，不牵扯到网络数据，网络数据只有下发给他，他跟网络的只有交互，
    /// 注意：服务器上也有该管理器
    /// </summary>
    public class LocalManager : MonoBehaviour
    {

        public static LocalManager Instance;


        public GameObject ovrCamRig;
        /// <summary>
        /// 世界中心点更改的事件
        /// </summary>
        public event Action<Vector3, Quaternion> WorldChangeEvent;

        public Transform HeadTransform;
        public Transform LeftHandTransform;
        public Transform RightHandTransform;

        public List<AudioInfo> AudioInfos = new List<AudioInfo>();

        /// <summary>
        /// 世界中心的偏移点，只作用变量MyWorldCenterPoint
        /// </summary>
        public Vector3 WorldOffestVector3 = new(0f, 0.0575f, 0f);

        /// <summary>
        /// 根物体的旋转
        /// </summary>
        public Quaternion RootQuaternion { get; private set; }


        /// <summary>
        /// 该程序打包的时候是否是安卓平板
        /// </summary>
        public bool IsAndroidPad = false;



        public UniversalRenderPipelineAsset AndroidPipelineAsset;

        public UniversalRendererData AndroidScriptableRendererData;

        public UniversalRenderPipelineAsset PCPipelineAsset;

        public AudioSource Audiosource { get; private set; }

        public Transform MyWorldCenterPoint { get; internal set; }

        /// <summary>
        /// 场景的根变换,很多的同步参考场景的根物体
        /// </summary>
        public Transform SceneRootTransform;


        /// <summary>
        /// 该程序打包的时候是否是第三摄像头程序
        /// </summary>
        public bool IsPCThird = false;

        public Material SelectMaterial;

        public GameObject LogGameObject;

        public GameObject LocalScene;

        /// <summary>
        /// 是否已经按下A 键
        /// </summary>
        private bool _isPressA;

        /// <summary>
        /// 是否已经按下B 键
        /// </summary>
        private bool _isPressB;

        private float _timeTemp = 0f;
        /// <summary>
        /// 是否激活偏移位置的操作
        /// </summary>
        private bool _isActiveOffest = false;


        private bool _isPressTrigger;

        private bool _isPressThumbsitick;



        private float _timeTempRHandTrigger = 0f;
        /// <summary>
        /// 
        /// </summary>
        private bool _isActive = false;
        /// <summary>
        /// 虚拟世界的根物体，跟中心点的偏移量
        /// </summary>
        public Vector3 WorldOffest;



        protected virtual void OnAwake()
        {
            if (Instance != null) throw new UnityException("已经存在本地单例");

            Instance = this;
        }
        private void Awake()
        {
            OnAwake();

           

          

            
        }

      

        public void ResetAudioInfo()
        {
            for (int i = 0; i < AudioInfos.Count; i++)
            {
                AudioInfos[i].IsPlayed = false;
            }

            //停止所有的协同程序
            StopAllCoroutines();
        }


        void OnEnable()
        {


            if (OpenCVMarkManager.Instance != null)
            {
                OpenCVMarkManager.Instance.OpenCvAnchor.TrackSuccessEvent += Instance_OpenCVTrackCompleted;
            }
            else
            {
                Debug.LogError("没有获取到OpenCVMarkManager单例");
            }


            if (AnchorManager.Instance != null)
            {
                AnchorManager.Instance.LoadWorldPointCompleted += Instance_LoadWorldPointCompleted;
            }
            else
            {
                Debug.LogError("没有获取到AnchorManager锚点单例，如果是在安卓平板的话，这个无视");
            }

            if (MyARcoreManager.Instance != null)
            {
                MyARcoreManager.Instance.AnchorCompleted += InitARCoreWorldPointCompleted;
            }
            else
            {
                Debug.LogError("没有获取到MyARcoreManager单例，如果是在quest眼镜或者pc的的话，这个无视");
            }

            Debug.Log($"OnEnable");

            if (IsAndroidPad)
            {

                Init();
            }
        }

       
        void OnDisable()
        {
            if (AnchorManager.Instance != null)
                AnchorManager.Instance.LoadWorldPointCompleted -= Instance_LoadWorldPointCompleted;

            if (MyARcoreManager.Instance != null)
            {
                MyARcoreManager.Instance.AnchorCompleted -= InitARCoreWorldPointCompleted;
            }

            if (OpenCVMarkManager.Instance != null)
            {
                OpenCVMarkManager.Instance.OpenCvAnchor.TrackSuccessEvent -= Instance_OpenCVTrackCompleted;
            }

        }

        void Start()
        {

          

            if (!IsAndroidPad)
            {
                Init();
            }
        }

        private void Update()
        {

            #region 偏移中心点的操作代码  和重置流程的代码

            if (BaseNetManager.FireCubeNetClient != null)//只有联网才能操作
            {
                if (OVRInput.GetDown(OVRInput.RawButton.A))
                {
                    _isPressA = true;
                   
                }
                else if (OVRInput.GetUp(OVRInput.RawButton.A))
                {
                    _isPressA = false;
                }


                if (OVRInput.GetDown(OVRInput.RawButton.A))
                {
                    _isPressB = true;
                }
                else if (OVRInput.GetUp(OVRInput.RawButton.B))
                {
                    _isPressB = false;
                }


                if (_isPressA && _isPressB)//如果同时按下AB键
                {
                    if (_timeTemp <= 5f)
                    {
                        _timeTemp += Time.deltaTime;
                    }
                    else
                    {
                        _timeTemp = 0f;
                        _isActiveOffest = !_isActiveOffest;//激活或者隐藏偏移指令
                        Debug.Log($"是否激活偏移设置：{_isActiveOffest}");



                        BaseNetManager.FireCubeNetClient?.ClientSceneNet.LogGameObject.SetActive(_isActiveOffest);



                        if (!_isActiveOffest)//如果取消偏移操作，则保存偏移到服务器，下发到各个客户端  
                        {

                            Debug.Log($"偏移值存储到本地:{WorldOffest}");

                            //存储到本地
                            PlayerPrefs.SetString("Offest", JsonUtility.ToJson(WorldOffest));
                        }
                        //再加个提示音
                    }
                }

                if (_isActiveOffest)
                {
                    Vector2 value = -OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick) * 0.001f;//*0.01不至于偏移过快  摇杆有时候是左摇杆可以起作用有时候是右摇杆起作用

                    Transform tipTransform = BaseNetManager.FireCubeNetClient?.ClientSceneNet.LogGameObject.transform;

                    Vector3 worldDir = tipTransform.TransformDirection(new Vector3(value.x, 0f, value.y));

                    WorldOffest += new Vector3(worldDir.x, 0f, worldDir.z);

                    Debug.Log($"WorldOffest:{WorldOffest}");

                    SceneRootTransform.position = MyWorldCenterPoint.position + WorldOffest;

                    BaseNetManager.FireCubeNetClient.ClientSceneNet.localBaseObject.transform.position = MyWorldCenterPoint.position + WorldOffest;
                }
            }
            else 

            {

                if (OVRInput.GetDown(OVRInput.RawButton.A))
                {
                    //重置旋转，如果本来有旋转的话
                    ovrCamRig.GetComponent<OVRCameraRig>().trackingSpace.rotation = Quaternion.identity;
                    //重置位置，如果位置不为0的话
                    ovrCamRig.GetComponent<OVRCameraRig>().trackingSpace.position = Vector3.zero;

                    LocalScene.SetActive(false);
                    OpenCVMarkManager.Instance.Reset();
                }

               
            }



            if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
            {
                _isPressTrigger = true;
            }
            else if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger))
            {
                _isPressTrigger = false;
            }


            if (OVRInput.GetDown(OVRInput.RawButton.RThumbstick))
            {
                _isPressThumbsitick = true;
            }
            else if (OVRInput.GetUp(OVRInput.RawButton.RThumbstick))
            {
                _isPressThumbsitick = false;
            }

            if (_isPressTrigger && _isPressThumbsitick)
            {
                if (_timeTempRHandTrigger >= 5f)
                {
                    _timeTempRHandTrigger = 0f;
                    _isPressTrigger = false;
                    _isPressThumbsitick = false;
                    //重置服务器指令
                    LocalForceStopServer();
                }
                else
                {
                    _timeTempRHandTrigger += Time.deltaTime;
                }
            }
            else
            {
                _timeTempRHandTrigger = 0f;
            }

            #endregion


            if (OVRPlugin.shouldRecenter)
            {
                Debug.Log($"重置中心点");
                //UnityEngine.XR.XRInputSubsystem currentInputSubsystem = OVRManager.GetCurrentInputSubsystem();
                //if (currentInputSubsystem != null)
                //{
                //    currentInputSubsystem.TryRecenter();
                //}
            }
        }



        /// <summary>
        /// 自动打开第三方摄像头程序
        /// </summary>
        public IEnumerator AutoOpenThirdCam()
        {
            //等待三秒，等待服务器启动完成，初始化完成  
            yield return new WaitForSeconds(3f);

            string thirdCamPath = Application.streamingAssetsPath + "/ThirdCam/GongWuDuan.exe";

            if (System.IO.File.Exists(thirdCamPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(thirdCamPath);

                Process.Start(startInfo);

            }
            else
            {
                Debug.LogError("File not found: " + thirdCamPath);
            }


        }



        private void Interactable_SelectEvent(GameObject obj)
        {
            Debug.Log($"选择了工具：{obj.name}");

            string[] contents = obj.name.Split("__", StringSplitOptions.None);

            //取数组第二个索引的值  
            if (contents.Length == 2)
            {
                string value = contents[1];

                int index = int.Parse(value);

                // ControlUIManager.instance?.OnSelectToolPanel(1, index);
            }
            else
            {
                Debug.Log($"选择的工具不符合规范:{obj.name}");
            }


        }
        private void Init()
        {
#if UNITY_ANDROID
            Debug.Log("RuntimePlatform.Android");

            // GraphicsSettings.renderPipelineAsset = AndroidPipelineAsset;
            // QualitySettings.renderPipeline = AndroidPipelineAsset;
#else

            Debug.Log("RuntimePlatform.PC");

            //GraphicsSettings.renderPipelineAsset = PCPipelineAsset;
            //QualitySettings.renderPipeline = PCPipelineAsset;

#endif




#if UNITY_SERVER
        StartCoroutine(AutoOpenThirdCam());
       
#endif

            //ShowStep00UI();

            Audiosource = this.GetComponent<AudioSource>();



        }


        /// <summary>
        /// 用arcore获取到的世界中心点加载完成
        /// </summary>
        private void InitARCoreWorldPointCompleted(Transform anchor)
        {

            Debug.Log($"用arcore获取到了锚点");



            RootQuaternion = anchor.rotation;



            Transform oldTransform = MyWorldCenterPoint;

            if (oldTransform != null) Destroy(oldTransform.gameObject);

            MyWorldCenterPoint = anchor;

            MyWorldCenterPoint.position += WorldOffestVector3;

            if (SceneRootTransform == null)
            {
                GameObject go = new GameObject();
                go.name = "SceneRootTransform";
                SceneRootTransform = go.transform;
            }

            SceneRootTransform.transform.position = MyWorldCenterPoint.position;
            SceneRootTransform.transform.rotation = MyWorldCenterPoint.rotation;


            if (NetworkClient.localPlayer == null)//如果离线，则连接网络 
                ConnectServer();
        }


        /// <summary>
        /// 中心点加载完成的事件
        /// </summary>
        /// <param name="isQuest">是否是quest3给的世界中心点,true为quest3给的，false为第三方摄像头给的中心点</param>
        public void Instance_OpenCVTrackCompleted(Transform anchor,bool isAndroid)
        {

            Debug.Log($"OpenCV锚点完成");


            MyWorldCenterPoint = anchor;

            //MyWorldCenterPoint.position += WorldOffestVector3;

            if (SceneRootTransform == null)
            {
                GameObject go = new GameObject();
                go.name = "SceneRootTransform";
                SceneRootTransform = go.transform;
            }
            WorldOffest = Vector3.zero;


            if (isAndroid)
            {
                //获取的坐标需要转换一下

                //拿场景内的围墙和场景内的中心点做参考，中心点的X轴跟围墙垂直，并且中心点在工具架子和围墙的里面
                //由此得出下面的转换  

                Vector3 up = MyWorldCenterPoint.forward;//规定 中心点的forward轴是在虚拟世界up轴

                Vector3 forward = -MyWorldCenterPoint.right;//规定 中心点的right轴为虚拟世界的forward轴

                Quaternion q = Quaternion.LookRotation(forward, up);

                string vector3Str = PlayerPrefs.GetString("Offest");

                if (!string.IsNullOrEmpty(vector3Str))
                {
                    Vector3 temp = JsonUtility.FromJson<Vector3>(vector3Str);

                    WorldOffest = temp;

                    Debug.Log($"从本地获取偏移值：{WorldOffest}");
                }

                RootQuaternion = q;


                SceneRootTransform.transform.rotation = q;

                SceneRootTransform.transform.position = MyWorldCenterPoint.position + WorldOffest;
            }
            else
            {
                SceneRootTransform.transform.rotation = MyWorldCenterPoint.rotation;
                SceneRootTransform.transform.position = MyWorldCenterPoint.position;
            }



            //识别后forwar和right轴组成的面不在水平面上，需要校正一下
            var angleVector3 = SceneRootTransform.rotation.eulerAngles;

            SceneRootTransform.rotation = Quaternion.Euler(0f, angleVector3.y, 0f);

           

            MoveRotationPlayer();

            SetLocaScenelPos(false,true);

            AnchorManager.Instance.CreatAnchorLocaltion(SceneRootTransform.gameObject,false);

            ConnectServer();
        }

        /// <summary>
        /// 中心点加载完成的事件
        /// </summary>
        /// <param name="isQuest">是否是quest3给的世界中心点,true为quest3给的，false为第三方摄像头给的中心点</param>
        public void Instance_LoadWorldPointCompleted(bool isQuest)
        {

            Debug.Log($"世界中心点加载完成 ");


            MyWorldCenterPoint = AnchorManager.Instance.WorldTransform;

            //MyWorldCenterPoint.position += WorldOffestVector3;

            if (SceneRootTransform == null)
            {
                GameObject go = new GameObject();
                go.name = "SceneRootTransform";
                SceneRootTransform = go.transform;
            }
            WorldOffest = Vector3.zero;

            if (isQuest)//quest获取的坐标需要转换一下
            {
                //拿场景内的围墙和场景内的中心点做参考，中心点的X轴跟围墙垂直，并且中心点在工具架子和围墙的里面
                //由此得出下面的转换  

                Vector3 up = MyWorldCenterPoint.forward;//规定 中心点的forward轴是在虚拟世界up轴

                Vector3 forward = -MyWorldCenterPoint.right;//规定 中心点的right轴为虚拟世界的forward轴

                Quaternion q = Quaternion.LookRotation(forward, up);

                string vector3Str = PlayerPrefs.GetString("Offest");

                if (!string.IsNullOrEmpty(vector3Str))
                {
                    Vector3 temp = JsonUtility.FromJson<Vector3>(vector3Str);

                    WorldOffest = temp;

                    Debug.Log($"从本地获取偏移值：{WorldOffest}");
                }

                RootQuaternion = q;


                SceneRootTransform.transform.rotation = q;


            }
            else
            {
                SceneRootTransform.transform.rotation = MyWorldCenterPoint.rotation;
            }

            SceneRootTransform.transform.position = MyWorldCenterPoint.position + WorldOffest;

            SetLocaScenelPos(false);


            ConnectServer();
        }

        /// <summary>
        /// 设置本地的场景位置
        /// </summary>
        /// <param name="isHide">是否隐藏,false为现实，true为隐藏</param>
        /// <param name="isOpenCV">是否使用opencv识别</param>
        public void SetLocaScenelPos(bool isHide,bool isOpenCV=false)
        {
            if (!isHide)
            {
                if (LocalScene != null)
                {
                    LocalScene.gameObject.SetActive(true);
                    if (!isOpenCV)
                    {
                        LocalScene.transform.position = SceneRootTransform.position;
                        LocalScene.transform.rotation = SceneRootTransform.rotation;
                    }
                  
                }
            }
            else
            {
                if (LocalScene != null)
                {
                    LocalScene.gameObject.SetActive(false);
                }
            }

        }

        /// <summary>
        /// 移动并旋转角色，使其适应unity的世界位置
        /// </summary>
        private void MoveRotationPlayer()
        {
            if (SceneRootTransform == null) return;

            Debug.Log($"对齐眼镜到世界");

            Vector3 pos = SceneRootTransform.position;

            float angle = SceneRootTransform.eulerAngles.y;

            Transform trackingSpace = ovrCamRig.GetComponent<OVRCameraRig>().trackingSpace;

            //重置旋转，如果本来有旋转的话
            trackingSpace.rotation = Quaternion.identity;
            //重置位置，如果位置不为0的话
            trackingSpace.position = Vector3.zero;

            Transform headTransform = ovrCamRig.GetComponent<OVRCameraRig>().centerEyeAnchor;

            //先获取头的位置到锚点位置的向量，y轴以锚点Y轴为准
            var dir = new Vector3(headTransform.position.x, pos.y, headTransform.position.z) - pos;

            //旋转跟踪空间，trackingSpace跟centerEyeAnchor是父子关系，trackingSpace是父,centerEyeAnchor是子
            trackingSpace.rotation = Quaternion.Euler(0f, -angle + 90f, 0f);//加90的原因是对齐Z轴


            //旋转完父物体后，dir也要跟着旋转，因为子物体centerEyeAnchor也会跟着旋转
            dir = trackingSpace.rotation * dir;

            // Debug.Log($"{trackingSpace.name} 的pos:{trackingSpace.position}  头部的pos：{headTransform.position}  二维码的位置：{pos}");

            //new Vector3(-headTransform.position.x,0f,-headTransform.position.z)是让子物centerEyeAnchor的世界坐标在000位置
            trackingSpace.position = new Vector3(-headTransform.position.x,0f,-headTransform.position.z) +dir;

            Debug.Log($"{trackingSpace.name} 跟踪点的pos:{trackingSpace.position}  旋转：{trackingSpace.eulerAngles}  头部的位置：{headTransform.position} 偏移量：{dir}");
        }
        /// <summary>
        /// 本地远程暴力关闭服务器
        /// 并在2秒后开始重连服务器
        /// 
        /// </summary>
        public virtual void LocalForceStopServer()
        {
            if (NetworkClient.localPlayer != null)
            {
                BaseNetPlayer baseNetPlayer = NetworkClient.localPlayer.GetComponent<BaseNetPlayer>();

                baseNetPlayer.ClientResetServer();
            }
        }

        private void ConnectServer()
        {
            //设置完中心点，则自动接入网络 
#if !UNITY_SERVER

            List<string> ips = GetIP();



            foreach (string ip in ips)
            {
                if (ip.Contains($"192.168.31"))
                {
                    string serverIp = "192.168.31.212";//测试Ip
                    //项目地服务器
                    FindObjectOfType<NetworkManager>().networkAddress = serverIp;
                    Debug.Log($"项目服务器IP地址：{serverIp}");
                }
            }


            StartClient();//网络版   
#endif
        }

        public void StartClient()
        {

            BaseNetManager netManager = FindObjectOfType<BaseNetManager>();

            if (netManager == null) throw new UnityException("$没有找到网络管理组件");
            Debug.Log($"StartClient");
            netManager.StartClient();//开启网络
        }

        public List<string> GetIP()
        {
            IPAddress[] ipAddresses = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList;

            List<string> ips = new List<string>();

            foreach (IPAddress address in ipAddresses)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ips.Add(address.ToString());
                }
            }

            return ips;
            //return System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
            //    .AddressList.First(
            //        f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            //    .ToString();
        }
        /// <summary>
        /// 设置偏移量
        /// </summary>
        /// <param name="localOffest"></param>
        public void SetOffest(Vector3 localOffest)
        {
            //转化为世界的偏移量 
            Vector3 worldOffest = MyWorldCenterPoint.TransformDirection(localOffest);

            WorldOffest = worldOffest;
            SceneRootTransform.position = MyWorldCenterPoint.position + WorldOffest;
        }

        /// <summary>
        /// 设置偏移量
        /// </summary>
        /// <param name="worldOffest"></param>
        public void LocalSetOffest(Vector3 worldOffest)
        {
            WorldOffest = worldOffest;
            SceneRootTransform.position = MyWorldCenterPoint.position + WorldOffest;
        }



        /// <summary>
        /// 播放网络接收到的音频文件
        /// </summary>
        /// <param name="audioName"></param>
        public void PlayAudioNet(string audioName)
        {

            foreach (AudioInfo audioInfo in AudioInfos)
            {
                if (audioInfo.StepName == audioName)
                {
                    if (Audiosource == null) Audiosource = this.GetComponent<AudioSource>();
                    Audiosource.PlayOneShot(audioInfo.AudioClip);
                    Debug.Log($"播放音频文件：{audioName}");

                }
            }

        }


        /// <summary>
        /// 等待一段时间做某事
        /// </summary>
        /// <param name="time"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public IEnumerator Wait(float time, Action action)
        {

            Debug.Log($"等待时间：{time}");
            yield return new WaitForSeconds(time);

            action?.Invoke();
        }

        public void WaitDo(float time, Action action)
        {
            StartCoroutine(Wait(time, action));
        }

        public IEnumerator WaitFrame(Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void WaitFrameDo(Action action)
        {
            StartCoroutine(WaitFrame(action));
        }

        /// <summary>
        /// 获取某段音频的时间，等待这段时间后做某事
        /// </summary>
        /// <param name="audioName"></param>
        /// <param name="action"></param>
        public void GetAudioTimeDo(AudioClip audioClip, Action action)
        {

            WaitDo(audioClip.length, action);
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        private void OnGUI()
        {

            if (GUI.Button(new Rect(0f, 200f, 100f, 100f), "SetCam"))
            {
                ovrCamRig.transform.position = new Vector3(-0.74f, 1.07f, 0.11f);
                ovrCamRig.transform.rotation = Quaternion.Euler(18.416f, 180f, 0f);
            }
        }
#endif

    }


    [Serializable]
    public class AudioInfo
    {

        /// <summary>
        /// 步骤的名字
        /// </summary>
        public string StepName;

        /// <summary>
        /// 步骤相应的音频
        /// </summary>
        public AudioClip AudioClip;

        /// <summary>
        /// 是否播放过
        /// </summary>
        public bool IsPlayed = false;
    }
}
