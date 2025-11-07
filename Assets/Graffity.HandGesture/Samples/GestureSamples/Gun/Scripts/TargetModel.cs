using R3;
using UnityEngine;

namespace Graffity.HandGesture.Sample.FingerGun
{
    public class TargetModel
    {
        public ReadOnlyReactiveProperty<Vector3> Position => m_position;
        private readonly ReactiveProperty<Vector3> m_position = new();

        private static readonly Vector2 TARGET_SPAWN_X_RANGE = new(-1f, 1f);
        private static readonly Vector2 TARGET_SPAWN_Y_RANGE = new(1f, 2f);
        private static readonly Vector2 TARGET_SPAWN_Z_RANGE = new(2.5f, 3f);

        public TargetModel()
        {
            InitializePosition();
        }

        public void HitBullet(Collision collision)
        {
            // 破壊処理は行わず、位置をリセット
            if (collision.collider.CompareTag("Bullet"))
            {
                InitializePosition();
            }
        }

        private void InitializePosition()
        {
            m_position.Value = new Vector3(
                Random.Range(TARGET_SPAWN_X_RANGE.x, TARGET_SPAWN_X_RANGE.y),
                Random.Range(TARGET_SPAWN_Y_RANGE.x, TARGET_SPAWN_Y_RANGE.y),
                Random.Range(TARGET_SPAWN_Z_RANGE.x, TARGET_SPAWN_Z_RANGE.y)
            );
        }
    }
}
