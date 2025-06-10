using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GongWuDuanAdvance
{
    public class RoleController : MonoBehaviour
    {
        // 移动速度
        private float moveSpeed = -10.0f;

        // 旋转速度
        private float rotationSpeed = 20.0f;

        // Update is called once per frame
        void Update()
        {


               // 向前移动
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
            }

            // 向后移动
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.Self);
            }

            // 向左移动
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.Self);
            }

            // 向右移动
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(Vector3.right * moveSpeed * Time.deltaTime, Space.Self);
            }


            // 向左旋转
            if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(Vector3.down * rotationSpeed * Time.deltaTime*5f, Space.Self);
            }

            // 向右旋转
            if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime*5f, Space.Self);
            }






        }
    }
}
