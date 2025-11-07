using R3;
using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine.XR; 
using System;
// ★修正点 1: 外部の Graffity.HandGesture 名前空間の HandInfo を使用
using Graffity.HandGesture; 


namespace Graffity.HandGesture.Sample.FingerGun
{
    // ★注意: HandType enum および HandInfo struct の定義は、このファイルから削除されました。

    public class BulletModel 
    {
        private Dictionary<string, GameObject> warpPointsMap = new(); 
        
        public ReadOnlyReactiveProperty<Vector3> Position => m_position;
        public ReadOnlyReactiveProperty<Vector3> MoveVec => m_moveVec;
        public ReadOnlyReactiveProperty<bool> IsActive => m_isActive;
        public ReadOnlyReactiveProperty<bool> CanShot => m_canShot;

        ReactiveProperty<Vector3> m_position = new();
        ReactiveProperty<Vector3> m_moveVec = new();
        ReactiveProperty<bool> m_isActive = new(false);
        ReactiveProperty<bool> m_canShot = new(true); 

        private int breakCount;
        const float SHOT_SPEED = 25f;

        const float SHOT_COOLDOWN_TIME = 10.0f;
        private float m_nextShotTime = 0f;
        
        public float RemainingCooldownTime => m_nextShotTime;

        
        public void Start()
        {
            for (int i = 1; i <= 2; i++) 
            {
                string inPointName = $"WarpInPoint{i}";
                GameObject outPoint = GameObject.Find($"WarpOutPoint{i}");
                if (outPoint != null)
                {
                    warpPointsMap.Add(inPointName, outPoint); 
                }
            }
        }

        // ★修正点 2: HandInfo は Graffity.HandGesture.HandInfo クラスを参照
        public void Shot(HandInfo handInfo) 
        {
            if (m_nextShotTime > 0f) return;

            // ★修正点 3: TryGetJointInfo は HandInfo クラスのメンバーに依存するため、
            // 呼び出しを TryGetJointInfo に戻す
            if (handInfo.TryGetJointInfo(UnityEngine.XR.Hands.XRHandJointID.IndexTip, out var jointInfo))
            {
                var position = jointInfo.CurrentPose.position;
                var moveVec = jointInfo.CurrentPose.rotation * Vector3.forward;
                
                breakCount = 0;

                m_isActive.Value = true;
                m_position.Value = position;
                m_moveVec.Value = moveVec.normalized * SHOT_SPEED;

                m_nextShotTime = SHOT_COOLDOWN_TIME;
                m_canShot.Value = false; 
            }
        }

        public void Update()
        {
            if (m_nextShotTime > 0f)
            {
                m_nextShotTime -= Time.deltaTime;
                
                if (m_nextShotTime <= 0f)
                {
                    m_nextShotTime = 0f;
                    m_canShot.Value = true;
                }
            }
        }

        public void Hit(Collision collision)
        {
            float pushForce = 10f;
            string colliderTag = collision.collider.tag;
            string colliderName = collision.collider.gameObject.name;
            var incomingVec = m_moveVec.Value;
            var normalVec = collision.contacts[0].normal;
                
            Rigidbody otherRigidbody = collision.collider.GetComponent<Rigidbody>();

            if (colliderTag == "Target" || colliderTag == "Wall" || colliderTag == "Floor")
            {
                breakCount += 1;
                m_moveVec.Value = Vector3.Reflect(incomingVec, normalVec);

                if (breakCount >= 5) 
                {
                    m_isActive.Value = false;
                }
            }
            else if (colliderTag == "WarpInPoint")
            {
                if (warpPointsMap.TryGetValue(colliderName, out GameObject outPointObject))
                {
                    m_position.Value = outPointObject.transform.position;
                    breakCount = 0;
                }
            }
            
             if (otherRigidbody != null)
            {
                otherRigidbody.AddForce(-normalVec * pushForce, ForceMode.Impulse);
            }
        }
    }
}