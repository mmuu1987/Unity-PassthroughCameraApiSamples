using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

namespace FireCubeBase
{
    /// <summary>
    /// 锚点管理器，负责创建，加载，抹除，删除，保存锚点
    /// 负责创建，加载，抹除，删除，保存  定位锚点
    /// 该管理器负责获取 WorldTransform  如果空间锚点不用
    /// 就用场景锚点来获取 WorldTransform
    /// 该管理器Awake执行顺序时-20,在Script Execution Order
    /// </summary>
    public class AnchorManager : MonoBehaviour
    {

        public static AnchorManager Instance;

        /// <summary>
        /// 利用场景锚点还是空间锚点来获取WorldTransform
        /// true为利用场景，false为利用空间锚点
        /// </summary>
        [Tooltip("true为使用空间(场景)来锚定世界位置和旋转，false来使用空间(锚点)来锚定世界位置和旋转")]
        public bool IsUseSceneAnchor;

        //todo other语义标签只能一个场景设置一个
        private MRUKAnchor _centerMrukAnchor;
        /// <summary>
        /// 锚点帮助提示的物体，锚点状态的时候显示
        /// </summary>
        private GameObject _anchorHelp;

        public GameObject CenterGameObject;


        /// <summary>
        /// 中心点获取完成的事件
        /// </summary>
        public event Action<bool> LoadWorldPointCompleted;

        public event Action<Transform> AnchorUpdateEvent; 

        [SerializeField]
        private Anchor _anchorPrefab;

        /// <summary>
        /// unity世界的原点变换,如果没有锚点，则用场景来设置该变换
        /// 
        /// 
        /// </summary>
        public Transform AxialTransform;
        /// <summary>
        /// 在场景里的锚点列表,不包括定位锚点
        /// </summary>
        public List<Anchor> AnchorList { get; } = new List<Anchor>();

        /// <summary>
        /// 当前创建的锚点
        /// </summary>
        public OVRSpatialAnchor CurOvrSpatialAnchor { get; private set; }

        private float _timeTemp = 0f;

        /// <summary>
        /// 该程序打包的时候是否是第三摄像头程序
        /// </summary>
        public bool IsPCThird = false;


        private GameObject _curTipGameObject;

        public Transform WorldTransform
        {
            get
            {


#if UNITY_STANDALONE_WIN && !UNITY_EDITOR//如果是pc非编辑器环境 那么世界中心点就是000点
          return AxialTransform;      
#endif
                if (_localtionAnchor != null) return _localtionAnchor.transform;
                return AxialTransform;
            }
        }


        /// <summary>
        /// 定位专用的锚点
        /// </summary>
        private Anchor _localtionAnchor;


        /// <summary>
        /// 定位专用锚点的UUID
        /// </summary>
        private string _localtionUuid;

        /// <summary>
        /// 定位锚点是否正在保存中进行时
        /// </summary>
        private bool _isSaveing;

        /// <summary>
        /// 定位锚点是否保存完成
        /// </summary>
        private bool _isSaveCompleted;

        private SpatialAnchorLoader _spatialAnchorLoader;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
                throw new UnityException($"已经有了锚点管理器单例，不允许再次创建");
            }

            Debug.Log($"赋值定位点单例");
            Instance = this;

            DontDestroyOnLoad(this.gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {
            if (IsUseSceneAnchor)
            {
#if UNITY_EDITOR || UNITY_ANDROID || !UNITY_SERVER
                if (MRUK.Instance != null && !IsPCThird)
                {
                    Debug.Log($"注册MRUK的事件");

                    MRUK.Instance.RegisterSceneLoadedCallback((() =>
                    {
                        Debug.Log($"场景空间加载完成");

                        UpdateScene(MRUK.Instance.GetCurrentRoom());

                        //List<MRUKRoom> rooms = MRUK.Instance.Rooms;

                        //MRUKRoom last = rooms[rooms.Count - 1];

                        //UpdateScene(last);

                    }));

                    MRUK.Instance.RoomUpdatedEvent.AddListener((UpdateScene));

                    MRUK.Instance.RoomCreatedEvent.AddListener((room =>
                    {
                        Debug.Log($"{GetType().Name} 创建了空间:{room.name}");
                    }));
                }
#endif
            }
            else
            {
                _spatialAnchorLoader = GetComponent<SpatialAnchorLoader>();
                _spatialAnchorLoader.LoadAction += LoadAction;

                //判断是否有定位锚点，有则加载定位锚点

                _localtionUuid = HasLocaltion();
                if (!string.IsNullOrEmpty(_localtionUuid))
                {
                    //有定位锚点，则加载
                    _spatialAnchorLoader.LoadAnchorsByUuid(_localtionUuid);
                }

                _isSaveCompleted = true;
            }



        }
        /// <summary>
        /// 锚点加载完成的事件
        /// </summary>
        /// <param name="anchor"></param>
        private void LoadAction(Anchor anchor)
        {
            Debug.Log($"{anchor.UUID} 锚点加载完毕");

            if (anchor.UUID != _localtionUuid)//定位锚点不加入到空间锚点中
                AnchorList.Add(anchor);
            else
            {
                //定位锚点的提示图像隐藏
                //anchor.transform.Find("Visual").gameObject.SetActive(false);
                Debug.Log($"该锚点为定位锚点{anchor.UUID}");

                if (_localtionAnchor != null)//如果不为Null说明已经加载过了，
                {
                    //直接删掉之前加载好的锚点
                    Destroy(_localtionAnchor.gameObject);
                    _localtionAnchor = null;
                }
                _localtionAnchor = anchor;

                _localtionAnchor.OnEaseEvent += EaseLocaltionCompleted;

                _localtionAnchor.transform.parent = this.transform;//避免切换场景的时候被删除掉
            }
        }


        public void Init(GameObject anchorHelp)
        {
            _anchorHelp = anchorHelp;
        }


        // Update is called once per frame  
        void Update()
        {
            if (_localtionAnchor != null)
            {
                if (_localtionAnchor.IsLocaltied())
                {
                    if (!_isSaveCompleted && !_isSaveing)
                    {
                        _isSaveing = true;
                        _localtionAnchor.OnSaveLocalButtonPressed();
                    }
                }
            }

            if (CurOvrSpatialAnchor != null)
            {
                if (_timeTemp >= 0.1f)
                {
                    _timeTemp = 0f;

                    //bool isLocalized = _curOvrSpatialAnchor.Localized;

                    //if (isLocalized)
                    //{
                    //    Debug.Log($"查询更新锚点状态：{isLocalized}");
                    //}

                    AnchorUpdateEvent?.Invoke(CurOvrSpatialAnchor.transform);
                }
                else
                {
                    _timeTemp += Time.deltaTime;
                }

            }
        }
        /// <summary>
        /// 创建定位锚点  
        /// </summary>
        public void CreatAnchorLocaltion()
        {
            PlaceAnchor(true);
        }

        /// <summary>
        /// 创建定位锚点  
        /// </summary>
        public void CreatAnchorLocaltion(GameObject target, bool isLocaltion)
        {
            
            HideAllAnchor();

            _anchorHelp = target;

            PlaceAnchor(isLocaltion);
        }

        /// <summary>
        /// 创建锚点，创建锚点的同时，不能同步保存锚点
        /// </summary>
        /// <param name="isLocaltion"></param>
        public void PlaceAnchor(bool isLocaltion)
        {
            if (_anchorHelp == null) throw new UnityException($"AnchorHelp 为null 试用AnchorManager之前，请调用Init");

            Anchor anchor = Instantiate(_anchorPrefab, _anchorHelp.transform.position, _anchorHelp.transform.rotation);

            CurOvrSpatialAnchor = anchor.GetComponent<OVRSpatialAnchor>();

            CurOvrSpatialAnchor.OnLocalize += OvrSpatialAnchor_OnLocalize;

            if (!isLocaltion)
            {
                AnchorList.Add(anchor);
            }
            else
            {
                if (_localtionAnchor == null)//运行时的时候第一次创建
                {
                    _localtionAnchor = anchor;

                    _localtionAnchor.OnSaveEvent += SaveLocaotionCompleted;
                    _localtionAnchor.OnEaseEvent += EaseLocaltionCompleted;

                    _isSaveCompleted = false;
                    _isSaveing = false;
                    _localtionAnchor.transform.parent = this.transform;//避免切换场景的时候被删除掉
                }
                else
                {
                    if (_isSaveCompleted)//如果上一个定位点已经保存完成  
                    {
                        Anchor oldAnchor = _localtionAnchor;
                        _localtionAnchor = anchor;

                        Debug.Log($"抹掉旧的定位锚点{oldAnchor.UUID}");
                        oldAnchor.OnEraseButtonPressed();//抹掉上一个定位点

                        _localtionAnchor.OnSaveEvent += SaveLocaotionCompleted;
                        _localtionAnchor.OnEaseEvent += EaseLocaltionCompleted;

                        _localtionAnchor.transform.parent = this.transform;//避免切换场景的时候被删除掉
                        _isSaveCompleted = false;
                        _isSaveing = false;
                    }
                    else
                    {
                        Debug.Log($"上一个锚点还没有创建完成，不能进行这个锚点的创建");

                        Destroy(anchor.gameObject);
                    }
                }
            }
        }
        private void OvrSpatialAnchor_OnLocalize(OVRSpatialAnchor.OperationResult obj)
        {
            Debug.Log($"锚点本地化：{obj}");
        }
        public void HideAllAnchor()
        {
            foreach (Anchor anchor in AnchorList)
            {
                Destroy(anchor.gameObject);
            }
            AnchorList.Clear();
            CurOvrSpatialAnchor = null;
        }
        /// <summary>
        /// 抹掉所有的锚点，抹掉后并删除场景中的所有锚点
        /// </summary>
        public void EraseAllAnchor()
        {
            foreach (Anchor anchor in AnchorList)
            {
                anchor.OnEraseButtonPressed();
            }
            HideAllAnchor();


        }

        /// <summary>
        /// 保存所有的锚点
        /// </summary>
        public void SaveAllAnchor()
        {
            foreach (Anchor anchor in AnchorList)
            {
                anchor.OnSaveLocalButtonPressed();
            }
        }
        public void LoadAllAnchor()
        {
            GetComponent<SpatialAnchorLoader>().LoadAnchorsByUuid();
        }


        /// <summary>
        /// 是否已经有了定位点
        /// </summary>
        /// <returns></returns>
        public string HasLocaltion()
        {
            string value = PlayerPrefs.GetString("Localtion");

            return value;
        }


        public void SaveLocaotionCompleted(string uuid, Anchor anchor)
        {
            string value = PlayerPrefs.GetString("Localtion");

            Debug.Log(string.IsNullOrEmpty(value) ? $"第一次记录定位点的信息到本地" : "覆盖掉定位点，重新定位,锚点保存完成");

            PlayerPrefs.SetString("Localtion", uuid);

            _localtionUuid = uuid;

            _isSaveCompleted = true;

            _isSaveing = false;
        }
        /// <summary>
        /// 定位锚点成功抹去的事件 
        /// </summary>
        public void EaseLocaltionCompleted(Anchor anchor)
        {
            Debug.Log($"抹掉锚点完成");

            Destroy(anchor.gameObject);
            //anchor.gameObject.SetActive(false);
        }

        #region 场景空间定位部分的代码

        /// <summary>
        /// 更新空间  
        /// </summary>
        private void UpdateScene(MRUKRoom room)
        {




            _centerMrukAnchor = null;
            MRUKAnchor floorAnchor = null;
            //Debug.Log($"空间场景加载完成");
            var anchors = room.Anchors;

            foreach (MRUKAnchor mrukAnchor in anchors)
            {
                MRUKAnchor.SceneLabels sceneLabels = mrukAnchor.GetLabelsAsEnum();

                if (sceneLabels == MRUKAnchor.SceneLabels.OTHER)
                {
                    //发现中心锚点 
                    Debug.LogError($"发现中心锚点，注意，other(其他) 语义标签只能一个场景设置一个");

                    _centerMrukAnchor = mrukAnchor;

                }

                if (sceneLabels == MRUKAnchor.SceneLabels.FLOOR)
                {
                    Debug.Log($"发现地板锚点"); //

                    floorAnchor = mrukAnchor;
                }
            }

            if (_centerMrukAnchor == null)
            {
                Debug.Log($"没有发现中心锚点");
            }
            else
            {

                //锚定坐标轴到中心锚点 
                AxialTransform.SetPositionAndRotation(new Vector3(_centerMrukAnchor.transform.position.x, floorAnchor.transform.position.y, _centerMrukAnchor.transform.position.z), _centerMrukAnchor.transform.rotation);

                Debug.Log($"{GetType().Name} 世界中心的位置为：{AxialTransform.position}  旋转为：{AxialTransform.rotation.eulerAngles}");


                //显示辅助物体
                if (CenterGameObject != null)
                    _curTipGameObject = Instantiate(CenterGameObject, AxialTransform.position, AxialTransform.rotation);
                //隐藏中心锚点
                LoadWorldPointCompleted?.Invoke(true);
                

                // _centerMrukAnchor.gameObject.SetActive(false);

            }

        }


        /// <summary>
        /// 加载空间
        /// </summary>
        public void LoadMRUKRoom()
        {


            Debug.Log($"重新加载场景");
            MRUK.Instance.ClearScene();
            _centerMrukAnchor = null;
            if (_curTipGameObject != null) Destroy(_curTipGameObject);

            MRUK.Instance.LoadSceneFromDevice();

            Debug.Log(MRUK.Instance.SaveSceneToJsonString(SerializationHelpers.CoordinateSystem.Unity));

            //MRUK.Instance.LoadSceneFromJsonString();
        }

        /// <summary>
        /// 第三摄像头的定位已经获取到
        /// </summary>
        public void GetThirdCamPos(Transform thirdCamTransform)
        {
            if (thirdCamTransform != null)
            {
                Debug.Log("第三摄像头的定位已经获取到");
                AxialTransform = thirdCamTransform;
                LoadWorldPointCompleted?.Invoke(false);
            }
            else
            {
                throw new UnityException("获取第三摄像头的定位的变换为Null");
            }
        }

        #endregion

    }

}

