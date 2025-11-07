using System;
using R3;
using UnityEngine;
using System.Linq; // Linqを使用する場合は必要

namespace Graffity.HandGesture.Sample.FingerGun
{
    public class BulletViewModel : BulletView.IViewModel, IDisposable
    {
        // View.IViewModel が要求するすべてのメンバーを定義

        public ReadOnlyReactiveProperty<Vector3> ShotPosition => m_shotPosition;
        public ReadOnlyReactiveProperty<bool> IsActive => m_isActive;
        // ★修正点 1: MoveVec メンバーを追加
        public ReadOnlyReactiveProperty<Vector3> MoveVec => m_moveVec; 

        ReactiveProperty<Vector3> m_shotPosition = new();
        ReactiveProperty<bool> m_isActive = new(false);
        // ★修正点 2: MoveVec の ReactiveProperty を追加
        ReactiveProperty<Vector3> m_moveVec = new(); 

        Action<Collision> m_onHit;
        CompositeDisposable m_disposable = new();


        public BulletViewModel()
        {
            m_shotPosition.AddTo(m_disposable);
            // MoveVec と IsActive も Dispose 対象に追加することが推奨されます。
            m_isActive.AddTo(m_disposable);
            m_moveVec.AddTo(m_disposable); 
        }


        public void Dispose()
        {
            if (!m_disposable.IsDisposed) m_disposable.Dispose();
        }


        public void Bind(BulletModel model)
        {
            m_shotPosition.Value = model.Position.CurrentValue;
            m_isActive.Value = model.IsActive.CurrentValue;
            // ★修正点 3: MoveVec の初期値と購読を追加
            m_moveVec.Value = model.MoveVec.CurrentValue; 

            model.Position.Subscribe(value => m_shotPosition.Value = value).AddTo(m_disposable);
            model.IsActive.Subscribe(value => m_isActive.Value = value).AddTo(m_disposable);
            // ★修正点 4: MoveVec の購読を追加
            model.MoveVec.Subscribe(value => m_moveVec.Value = value).AddTo(m_disposable); 

            // ★修正点 5: HitTarget メソッド名が間違っている可能性があるため、Hit に変更
            // BulletModel の衝突処理メソッド名は Hit(Collision) または HitTarget(Collision) のいずれかです。
            // BulletModelに HitTarget が存在しない場合は、Hit に修正します。
            m_onHit = model.Hit; 
        }


        public void Hit(Collision collision)
        {
            m_onHit?.Invoke(collision);
        }


    }


}