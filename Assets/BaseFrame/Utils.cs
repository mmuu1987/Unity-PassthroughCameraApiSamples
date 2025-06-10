using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

using System;

namespace  FireCubeBase
{
    public class Utils
    {
        // 显示或隐藏某一物体
        public static void ShowOrHideGameObject(GameObject gameObject, bool active)
        {
            if (gameObject != null)
            {
                gameObject.SetActive(active);
            }
        }
        public static void ShowOrHideGameObject(Transform transform, bool active)
        {
            if (transform != null)
            {
                ShowOrHideGameObject(transform.gameObject, active);
            }
        }

        // 隐藏列表中无指定的物体  
        public static void HideWithoutAppointObjectInList(List<GameObject> list, string gameObjectName)
        {
            if (list != null && list.Count > 0 && !string.IsNullOrEmpty(gameObjectName))
            {
                foreach (GameObject gameObject in list)
                {
                    if (gameObject != null)
                    {
                        gameObject.SetActive(gameObject.name == gameObjectName);
                    }
                }
            }
        }

        // 显示或隐藏全部孩子
        public static void ShowOrHideAllChildren(Transform transform, bool active)
        {
            if (transform != null && transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; ++i)
                {
                    transform.GetChild(i).gameObject.SetActive(active);
                }
            }
        }
        public static void ShowOrHideAllChildren(GameObject gameObject, bool active)
        {
            if (gameObject != null)
            {
                ShowOrHideAllChildren(gameObject.transform, active);
            }
        }

        //隐藏或显示所有非指定的孩子
        public static void ShowOrHideAllChildren(Transform transform, bool active, string chilName, bool chilActive)
        {
            if (transform != null && transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; ++i)
                {
                    transform.GetChild(i).gameObject.SetActive(transform.GetChild(i).name == chilName ? chilActive : active);
                }
            }
        }

        //显示或隐藏名字一样的孩子
        private const string k_ITEM_NAME = "ItemName";
        public static void ShowOrHidePartChildren(Transform transform, bool active)
        {
            List<Transform> allchildrenList = new List<Transform>();
            if (transform != null && transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; ++i)
                {
                    Transform child = transform.GetChild(i);
                    if (child != null && child.name == k_ITEM_NAME)
                    {
                        child.gameObject.SetActive(active);
                    }
                }
            }
        }

        // 显示或隐藏物体列表
        public static void ShowOrHideGameObjectList(List<GameObject> gameObjectList, bool active)
        {
            if (gameObjectList != null && gameObjectList.Count > 0)
            {
                foreach (GameObject gameObject in gameObjectList)
                {
                    if (gameObject != null)
                    {
                        gameObject.SetActive(active);
                    }
                }
            }
        }

        // 显示或隐藏物体列表
        public static void ShowOrHideTransformList(List<Transform> transformList, bool active)
        {
            if (transformList != null && transformList.Count > 0)
            {
                foreach (Transform transform in transformList)
                {
                    if (transform != null)
                    {
                        transform.gameObject.SetActive(active);
                    }
                }
            }
        }

        public static void ShowOrHideTextList(List<Text> transformList, bool active)
        {
            if (transformList != null && transformList.Count > 0)
            {
                foreach (Text transform in transformList)
                {
                    if (transform != null)
                    {
                        transform.gameObject.SetActive(active);
                    }
                }
            }
        }

        public static void ShowOrHideButtonList(List<Button> transformList, bool active)
        {
            if (transformList != null && transformList.Count > 0)
            {
                foreach (Button transform in transformList)
                {
                    if (transform != null)
                    {
                        transform.gameObject.SetActive(active);
                    }
                }
            }
        }

        public static GameObject GetObjectInList(List<GameObject> gameObjectList, string objectName)
        {
            if (gameObjectList != null && gameObjectList.Count > 0 && !string.IsNullOrEmpty(objectName))
            {
                foreach (GameObject obj in gameObjectList)
                {
                    if (obj != null && obj.name == objectName)
                    {
                        return obj;
                    }
                }
            }
            return null;
        }

        public static Transform GetTransformnList(List<Transform> parentTransformList, string childName)
        {
            if (parentTransformList != null && parentTransformList.Count > 0 && !string.IsNullOrEmpty(childName))
            {
                foreach (Transform tra in parentTransformList)
                {
                    if (tra != null && tra.name == childName)
                    {
                        return tra;
                    }
                }
            }
            return null;
        }

        // 获得Toogle按钮列表
        public static List<Transform> GetToggleItemTransformList<T>(Transform toggleGroupTransform)
        {
            List<Transform> resultList = new List<Transform>();
            if (toggleGroupTransform != null && toggleGroupTransform.childCount > 0)
            {
                for (int i = 0; i < toggleGroupTransform.childCount; ++i)
                {
                    Transform child = toggleGroupTransform.GetChild(i);
                    if (child != null && child.GetComponent<T>() != null)
                    {
                        resultList.Add(child);
                    }
                }
            }
            return resultList;
        }

        // 通过父亲获得孩子列表
        public static List<Transform> GetChildrenTransformList(Transform parentTransform)
        {
            List<Transform> resultList = new List<Transform>();
            if (parentTransform != null && parentTransform.childCount > 0)
            {
                for (int i = 0; i < parentTransform.childCount; ++i)
                {
                    resultList.Add(parentTransform.GetChild(i));
                }
            }
            return resultList;
        }
        // 通过父亲列表获得孩子列表
        public static List<Transform> GetChildrenTransformList(List<Transform> parentTransformList)
        {
            List<Transform> resultList = new List<Transform>();
            if (parentTransformList != null && parentTransformList.Count > 0)
            {
                List<Transform> childResultList;
                for (int i = 0; i < parentTransformList.Count; ++i)
                {
                    childResultList = GetChildrenTransformList(parentTransformList[i]);
                    if (childResultList != null && childResultList.Count > 0)
                    {
                        resultList.AddRange(childResultList);
                    }
                }
            }
            return resultList;
        }

        // 通过父亲查找孩子
        public static Transform FindChildByName(Transform parentTransform, string childName)
        {
            if (parentTransform != null && parentTransform.childCount > 0 && !string.IsNullOrEmpty(childName))
            {
                for (int i = 0; i < parentTransform.childCount; ++i)
                {
                    if (parentTransform.GetChild(i).name == childName)
                    {
                        return parentTransform.GetChild(i);
                    }
                }
            }
            return null;
        }

        // 通过父列表查找孩子
        public static Transform FindChildByName(List<Transform> parentTransformList, string childName)
        {
            if (parentTransformList != null && parentTransformList.Count > 0 && !string.IsNullOrEmpty(childName))
            {
                Transform result = null;
                for (int i = 0; i < parentTransformList.Count; ++i)
                {
                    Transform parentTransform = parentTransformList[i];
                    if (parentTransform != null)
                    {
                        result = FindChildByName(parentTransform, childName);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            return null;
        }
        // 通过父列表查找孩子
        public static Transform FindChildByName(List<GameObject> parentTransformList, string childName)
        {
            if (parentTransformList != null && parentTransformList.Count > 0 && !string.IsNullOrEmpty(childName))
            {
                Transform result = null;
                for (int i = 0; i < parentTransformList.Count; ++i)
                {
                    Transform parentTransform = parentTransformList[i].transform;
                    if (parentTransform != null)
                    {
                        result = FindChildByName(parentTransform, childName);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            return null;
        }
        public static bool IsHasChildInParent(Transform parent, string childName)
        {
            if (parent != null && parent.childCount > 0)
            {
                foreach (Transform trans in parent)
                {
                    if (trans.name == childName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        // 通过物体父类查找所以含有相同类型的列表
        public static List<T> GetGameObjects<T>(Transform parent)
        {
            List<T> gameObjects = new List<T>();
            T[] itemGuideWaveAnimations = parent.GetComponentsInChildren<T>(true);
            if (itemGuideWaveAnimations != null && itemGuideWaveAnimations.Length > 0)
            {
                foreach (T item in itemGuideWaveAnimations)
                {
                    if (item != null)
                    {
                        gameObjects.Add(item);
                    }
                }
                return gameObjects;
            }
            return gameObjects;
        }
        public static List<GameObject> GetListItemContainsName(List<GameObject> list, string name)
        {
            List<GameObject> myList = new List<GameObject>();
            foreach (GameObject go in list)
            {
                if (go.name.Contains(name))
                {
                    myList.Add(go);
                }
            }
            return myList;
        }

        public static void SetGameObjectsMaterail(List<GameObject> list, Material material)
        {
            if (list.Count > 0)
            {
                foreach (GameObject go in list)
                {
                    if (go.GetComponent<Renderer>() != null)
                    {
                        go.GetComponent<Renderer>().material = material;
                    }
                }
            }
        }

        public static void OpenOrCloseColliderInList(List<GameObject> objectList, bool isOpen)
        {
            if (objectList.Count > 0)
            {
                foreach (GameObject go in objectList)
                {
                    if (go.GetComponent<Collider>() != null)
                    {
                        go.GetComponent<Collider>().enabled = isOpen;
                    }
                }
            }
        }

        /*
            How to project a point on to a sphere
            https://stackoverflow.com/questions/9604132/how-to-project-a-point-on-to-a-sphere

            For the simplest projection (along the line connecting the point to the center of the sphere):
            Write the point in a coordinate system centered at the center of the sphere (x0,y0,z0):
            P = (x',y',z') = (x - x0, y - y0, z - z0)
            Compute the length of this vector:
            |P| = sqrt(x'^2 + y'^2 + z'^2)
            Scale the vector so that it has length equal to the radius of the sphere:
            Q = (radius/|P|)*P
            And change back to your original coordinate system to get the projection:
            R = Q + (x0,y0,z0)
        */
        // 通过手柄当前位置及球心半径的位置，获得手柄在球上的投影坐标，也就是图文菜单应该出现的位置
        public static Vector3 GetControllerProjectionPositionOnSphere(Vector3 sphereCenter, float radius, Vector3 controllerPosition)
        {
            Vector3 P = controllerPosition - sphereCenter;
            Vector3 Q = (radius / P.magnitude) * P;
            Vector3 R = Q + sphereCenter;
            return R;
        }

        private static readonly string TRIM_SUFFIX_NUMBER_PATTERN = @"\d+$";
        private static readonly Regex TRIM_SUFFIX_NUMBER_REGEX = new Regex(TRIM_SUFFIX_NUMBER_PATTERN);
        public static string GetColliderNameWithoutSuffixIndex(string colliderName)
        {
            if (string.IsNullOrEmpty(colliderName))
            {
                return string.Empty;
            }
            return TRIM_SUFFIX_NUMBER_REGEX.Replace(colliderName, string.Empty);
        }

        // 自定义版本的判断字符串以什么开始
        public static bool StringEndsWith(string str, string suffix)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(suffix))
            {
                return false;
            }
            int ap = str.Length - 1;
            int bp = suffix.Length - 1;

            while (ap >= 0 && bp >= 0 && str[ap] == suffix[bp])
            {
                ap--;
                bp--;
            }
            return (bp < 0 && str.Length >= suffix.Length) || (ap < 0 && suffix.Length >= str.Length);
        }

        // 自定义版本的判断字符串以什么结尾
        public static bool StringStartsWith(string str, string prefix)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(prefix))
            {
                return false;
            }
            int aLen = str.Length;
            int bLen = prefix.Length;
            int ap = 0; int bp = 0;

            while (ap < aLen && bp < bLen && str[ap] == prefix[bp])
            {
                ap++;
                bp++;
            }

            return (bp == bLen && aLen >= bLen) || (ap == aLen && bLen >= aLen);
        }

        public static void NewOrClearList<T>(ref List<T> list)// 对列表进行创建或者清空操作
        {
            if (list == null)
            {
                list = new List<T>();
            }
            else
            {
                list.Clear();
            }
        }



        /// <summary>  
        /// Unix时间戳转为C#格式时间  
        /// </summary>  
        /// <param name="timeStamp">Unix时间戳格式,例如1482115779</param>  
        /// <returns>C#格式时间</returns>  
        public static DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }


        /// <summary>  
        /// DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time"> DateTime时间格式</param>  
        /// <returns>Unix时间戳格式</returns>  
        public static int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
    }

}
