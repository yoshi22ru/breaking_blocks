using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace Graffity.HandGesture.Sample.FingerGun
{
    public class SampleFingerGunModel : IDisposable
    {
        public IReadOnlyList<BulletModel> BulletModelList => m_bulletModelList;
        public IReadOnlyList<TargetModel> TargetModelList => m_targetModelList;

        private readonly List<BulletModel> m_bulletModelList = new();
        private readonly List<TargetModel> m_targetModelList = new();
        private readonly CompositeDisposable m_disposable = new();

        private const int BULLET_MAX = 30;
        private const int TARGET_MAX = 5;

        // ReactiveProperty: 残弾数
        private readonly ReactiveProperty<int> _remainingShots = new(BULLET_MAX);
        public ReadOnlyReactiveProperty<int> RemainingShots => _remainingShots;

        // ステージクリア状態
        private readonly ReactiveProperty<bool> _isStageCleared = new(false);
        public ReadOnlyReactiveProperty<bool> IsStageCleared => _isStageCleared;

        public SampleFingerGunModel()
        {
            // 弾の初期化
            for (int i = 0; i < BULLET_MAX; i++)
            {
                var bulletModel = new BulletModel();
                m_bulletModelList.Add(bulletModel);
            }

            // ターゲット初期化
            for (int i = 0; i < TARGET_MAX; i++)
            {
                var targetModel = new TargetModel();
                m_targetModelList.Add(targetModel);
            }
        }

        public void Update()
        {
            foreach (var bulletModel in m_bulletModelList)
                bulletModel.Update();
        }

        public void Dispose()
        {
            if (!m_disposable.IsDisposed)
                m_disposable.Dispose();
        }

        // 修正版: HandInfoをそのまま渡す
        public void Shot(HandInfo hand)
        {
            // 弾が撃てるか確認
            var bulletModel = m_bulletModelList
                .FirstOrDefault(b => !b.IsActive.CurrentValue); // ← 修正ポイント

            if (bulletModel == null)
                return;

            // HandInfoをそのまま渡す（BulletModel側でIndexTipなどを取る）
            bulletModel.Shot(hand);

            // 残弾数を減らす
            _remainingShots.Value = Mathf.Max(0, _remainingShots.Value - 1);
        }

        public bool IsAnyBulletReadyToShot()
        {
            // ReactivePropertyの現在値を参照する
            return m_bulletModelList.Any(b => !b.IsActive.CurrentValue); // ← 修正ポイント
        }
    }
}
