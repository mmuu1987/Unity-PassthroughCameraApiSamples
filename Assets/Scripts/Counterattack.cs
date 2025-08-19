using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Counterattack : MonoBehaviour
{

    private Vector3 collisionPoint;      // 碰撞点
    private Vector3 postCollisionVelocity; // 碰撞后的速度
    private float collisionTime;        // 碰撞发生的时间
    private bool hasCollided;            // 碰撞标志位

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        // 1. 记录碰撞信息
        collisionPoint = collision.contacts[0].point;
        postCollisionVelocity = GetComponent<Rigidbody>().velocity;
        collisionTime = Time.time;
        hasCollided = true;

        Debug.Log($"碰撞发生在: {collisionTime}, 位置: {collisionPoint}, 碰撞后速度: {postCollisionVelocity}");
    }

    /// <summary>
    /// 计算碰撞后指定时间点的位置
    /// </summary>
    /// <param name="secondsAfterCollision">碰撞后的时间偏移（秒）</param>
    public Vector3 CalculatePosition(float secondsAfterCollision)
    {
        if (!hasCollided)
        {
            Debug.LogWarning("未发生碰撞，无法计算位置");
            return transform.position;
        }

        // 2. 获取物理参数
        Vector3 gravity = Physics.gravity; // Unity 的重力加速度（默认为 (0, -9.81, 0)）
        float t = secondsAfterCollision;

        // 3. 计算各轴位移（X/Z轴匀速，Y轴匀加速）S=v0t+0.5*a*t*t;
        float x = collisionPoint.x + postCollisionVelocity.x * t;
        float y = collisionPoint.y + postCollisionVelocity.y * t + 0.5f * gravity.y * t * t;
        float z = collisionPoint.z + postCollisionVelocity.z * t;

        return new Vector3(x, y, z);
    }

    

    public Vector3 GetState(Vector3 colliderPos,Vector3 velocity,Vector3 targetPos)
    {
        float xLength = targetPos.x - colliderPos.x;

        float xSpeed = velocity.x;

        float t = xLength / xSpeed;

        Vector3 displacement = velocity * t + 0.5f * Physics.gravity * (t * t);


        return colliderPos + displacement;



    }

    public void GetState(Vector3 colliderPos, int state, Vector3 dir)
    {

        float tableLength = 2.17f;

        Transform tableTransform =null;

        if (state == 1)//
        {

            Vector3 tempDir = new Vector3(dir.x, 0f, dir.z);

            tempDir = tempDir.normalized;

            Vector3 pos = colliderPos + tempDir * tableLength;

            pos.y = tableTransform.position.y;

            MoveToTarget(pos);

        }
        else if (state == 2)
        {
           
            
        }
    }

    public Vector3 GetPos(Vector3 v,float t,Vector3 a)
    {
        Vector3 displacement = v * t + 0.5f * a * (t * t);

        return displacement;
    }

    public void MoveToTarget(Vector3 target)
    {
        this.transform.DOMove(target, 0.35f);
    }
}
