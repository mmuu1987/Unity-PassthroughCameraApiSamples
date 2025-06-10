using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;


namespace FireCubeBase
{
    /// <summary>
    /// 基本的定位同步功能
    /// </summary>
    public class BaseNet : NetworkBehaviour
    {
        /// <summary>
        /// 该变换跟中心点的的偏移量,用来定位置
        /// </summary>
        [SyncVar]
        public Vector3 Pos_Dir;

        /// <summary>
        /// 该向量为相对于中心点变换的前向向量，用来定方向
        /// </summary>
        [SyncVar]
        public Vector3 Forward;

        /// <summary>
        /// 该向量为相对于中心点变换的向上向量，用来定方向
        /// </summary>
        [SyncVar]
        public Vector3 Up;

        /// <summary>
        /// 世界的中心点
        /// </summary>
        public Transform WorldCenterPoint;

        [SyncVar]
        public float LerpSpeed = 8f;



        /// <summary>
        /// 是否改变高度，添加服务器配置表里的高度，在服务器实例化的时候
        /// </summary>
        public bool IsChangeHeight = false;
       

        protected AudioSource AudioSource;

        private Vector3 _localPosDir;


        /// <summary>
        /// 该网络物体所在的本地物体
        /// </summary>
        public LocalBaseObject localBaseObject { get; private set; }


        #region 双端都可能调用的代码

        private void Awake()
        {
            OnAwake();
        }
        protected virtual void OnAwake()
        {
            localBaseObject = this.GetComponent<LocalBaseObject>();

           

            WorldCenterPoint = LocalManager.Instance.SceneRootTransform;

            AudioSource = this.GetComponent<AudioSource>();
        }

        protected virtual void OnStart()
        {

        }
        private void Start()
        {
            OnStart();
        }

        protected virtual void OnUpDate()
        {
            if (isClient)
            {
                if (isOwned)
                {
                    //计算跟全局参考点的偏移量
                    GetDir();
                }
                else
                {
                    //获取同步变量的值，设置头，头的位置   

                    if (WorldCenterPoint != null)
                    {

                        if (_localPosDir == Vector3.zero) _localPosDir = Pos_Dir;
                        else
                        {
                            _localPosDir = Vector3.Lerp(_localPosDir, Pos_Dir, Time.deltaTime * LerpSpeed);
                        }


                        this.transform.position = WorldCenterPoint.position + WorldCenterPoint.TransformDirection(_localPosDir);

                        if (Forward == Vector3.zero || Up == Vector3.zero) return;

                        this.transform.rotation = Quaternion.LookRotation(WorldCenterPoint.TransformDirection(Forward), WorldCenterPoint.TransformDirection(Up));
                    }
                }
            }
        }
        void Update()
        {
            OnUpDate();

        }

      
        [ClientRpc]
        public void RpcUpdatePlayAudio(string audioName,bool isLoop)
        {
            PlayAudio(audioName, isLoop, false);
        }

        /// <summary>
        /// 本地播放音频
        /// </summary>
        /// <param name="audioName"></param>
        /// <param name="isServerPlay">是否是在服务器端调用</param>
        public virtual void PlayAudio(string audioName,bool isLoop=false,bool isServerPlay=false)
        {
            int index = -1;
            foreach (AudioInfo audioInfo in LocalManager.Instance.AudioInfos)
            {
                if (audioInfo.StepName == audioName)
                {
                    if (!isServer)
                    {
                        Debug.Log($"播放音频：{audioName}");
                        AudioSource.clip = audioInfo.AudioClip;
                        AudioSource.Play();
                    }
                  
                    LocalManager.Instance.GetAudioTimeDo(audioInfo.AudioClip, (() =>
                    {
                        //音频结束后触发的事件
                        AudioEndPlay(audioName, isServerPlay);
                    }));

                    index = LocalManager.Instance.AudioInfos.IndexOf(audioInfo);
                }
            }

            if (isServer)
            {
                if(index>0)
                 LocalManager.Instance.AudioInfos[index].IsPlayed = true;
            }


         

        }
        public virtual void PlayLoopAudio(string audioName)
        {
           
        }
        /// <summary>
        /// 停止循环提示音
        /// </summary>
        /// <param name="audioName"></param>
        [ClientRpc]
        public void RPCStopLoopAudio(string audioName)
        {
            StopLoopAudio(audioName);
        }
        protected Coroutine _coroutine;

        public virtual void StopLoopAudio(string audioName)
        {
            
        }


        /// <summary>
        /// 获取音频的时长
        /// </summary>
        /// <param name="audioName"></param>
        /// <returns></returns>
        public float GetAudioTime(string audioName)
        {
            foreach (AudioInfo audioInfo in LocalManager.Instance.AudioInfos)
            {
                if (audioInfo.StepName == audioName)
                {
                   return audioInfo.AudioClip.length;
                }
            }

            return 0f;
        }
        #endregion




        #region 客户端调用的代码
        /// <summary>
        /// 语音播放结束触发
        /// </summary>
        /// <param name="stepEnd"></param>
        public virtual void AudioEndPlay(string stepEnd,bool isServerPlay=false)
        {

        }

        protected virtual void GetDir()
        {

            if (WorldCenterPoint != null)
            {
                Pos_Dir = WorldCenterPoint.InverseTransformDirection(this.transform.position - WorldCenterPoint.position);

                Forward = WorldCenterPoint.InverseTransformDirection(this.transform.forward);
                Up = WorldCenterPoint.InverseTransformDirection(this.transform.up);
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (isOwned)
            {
                this.name += "_Owned";
            }
            else
            {
                this.name += "_Observers";
            }

        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

        }

        /// <summary>
        /// 提交播放语音
        /// </summary>
        /// <param name="audioName"></param>
        [Command]
        public void ComSetSyncAudioName(string audioName)
        {
            Debug.Log($"ComSetSyncAudioName:{audioName}");

            //判断是否播放过
            foreach (AudioInfo info in LocalManager.Instance.AudioInfos)
            {
                if (info.StepName == audioName)
                {
                    if (!info.IsPlayed)
                    {

                        RpcUpdatePlayAudio(audioName,false);

                        //服务器这边检测是否播放完毕,
                        PlayAudio(audioName,false,true);
                    }
                    else
                    {
                        Debug.Log($"该语音已经播放过了");
                    }
                }
            }


        }

        #endregion


        #region 服务端代码


        /// <summary>
        /// 客户端获取到权限
        /// 该方法运行再服务端
        /// </summary>
        public virtual void ClientGetAuthority()
        {
            Debug.Log($"在服务端，该实例授权给了客户端某个玩家");
        }

        #endregion

    }
}

   
