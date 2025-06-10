using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace FireCubeBase
{
    public class MyARcoreManager : MonoBehaviour
    {

        public static MyARcoreManager Instance;

        public event Action<Transform> AnchorCompleted;


        public ARTrackedImageManager ArTrackedImageManager;

        public ARAnchorManager ArAnchorManager;

        [SerializeField]
        [Tooltip("The prefab to be instantiated for each anchor.")]
        GameObject m_Prefab;


        private void Awake()
        {
            Instance = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            ArTrackedImageManager.trackedImagesChanged += ArTrackedImageManager_trackedImagesChanged;
        }

        private void ArTrackedImageManager_trackedImagesChanged(ARTrackedImagesChangedEventArgs obj)
        {

            foreach (ARTrackedImage image in obj.updated)
            {
                UpdateInfo(image);

                AddAnchor(image.transform);
            }
        }
        void UpdateInfo(ARTrackedImage trackedImage)
        {



            var text = string.Format(
                "{0}\ntrackingState: {1}\nGUID: {2}\nReference size: {3} cm\nDetected size: {4} cm",
                trackedImage.referenceImage.name,
                trackedImage.trackingState,
                trackedImage.referenceImage.guid,
                trackedImage.referenceImage.size * 100f,
                trackedImage.size * 100f);




            Debug.Log($"trackingState:{trackedImage.trackingState}.inf:{text}.position:{trackedImage.transform.position}");



        }


        public ARAnchor ArAnchor { get; private set; }

        public void AddAnchor(Transform AnchorTransform)
        {

            if (ArAnchor == null)
            {
                var anchorPrefab = Instantiate(m_Prefab, AnchorTransform.position, AnchorTransform.rotation);
                ArAnchor = ComponentUtils.GetOrAddIf<ARAnchor>(anchorPrefab, true);

                Debug.Log($"添加一个锚点 :{AnchorTransform.position}  {AnchorTransform.rotation}");
            }
            else
            {
                ArAnchor.transform.position = AnchorTransform.position;
            }


            if (ArAnchorManager.subsystem != null)
                ArTrackedImageManager.subsystem.Stop();

            AnchorCompleted?.Invoke(ArAnchor.transform);




        }

        public IEnumerator WaitDo(float time, Action action)
        {
            yield return new WaitForSeconds(time);

            action?.Invoke();
        }
        // Update is called once per frame
        void Update()
        {
            // Debug.Log($"锚点位置：{ArAnchor.transform.position}");
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(0f, 100f, 100f, 100f), "重新锚点"))
            {
                if (ArAnchorManager.subsystem != null)
                    ArTrackedImageManager.subsystem.Start();
            }
        }
    }

}

