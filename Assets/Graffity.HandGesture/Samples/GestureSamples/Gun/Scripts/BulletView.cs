using R3;
using UnityEngine;

namespace Graffity.HandGesture.Sample.FingerGun
{
    // ★注意: このIViewModelの定義は、プロジェクト内の他のView/ViewModelファイルと一致している必要があります
    public class BulletView : MonoBehaviour
    {
        public interface IViewModel
        {
            ReadOnlyReactiveProperty<Vector3> ShotPosition { get; }
            ReadOnlyReactiveProperty<bool> IsActive { get; }
            ReadOnlyReactiveProperty<Vector3> MoveVec { get; }
            void Hit(Collision collision);
        }

        [SerializeField] private GameObject decalPrefab;
        [SerializeField] private float decalLifeTime = 5f;

        private Rigidbody m_rigidbody;
        private IViewModel m_viewModel;

        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody>();
        }

        public void Bind(IViewModel vm)
        {
            m_viewModel = vm;
    
            vm.ShotPosition
                .Subscribe(value => 
                {
                    if (m_rigidbody == null) return;
                    
                    Vector3 storedVelocity = m_rigidbody.linearVelocity;
                    m_rigidbody.isKinematic = true;
                    transform.position = value;
                    m_rigidbody.isKinematic = false;
                    m_rigidbody.linearVelocity = storedVelocity; 
                    
                    if (storedVelocity.sqrMagnitude > 0.001f)
                    {
                        transform.rotation = Quaternion.LookRotation(storedVelocity);
                    }
                })
                .RegisterTo(this.destroyCancellationToken);

            vm.IsActive
                .Subscribe(isActive => {
                    gameObject.SetActive(isActive);
                    if (isActive)
                    {
                        if (m_rigidbody != null)
                        {
                            m_rigidbody.linearVelocity = vm.MoveVec.CurrentValue;
                        }
                    }
                })
                .RegisterTo(this.destroyCancellationToken);

            vm.MoveVec
                .Subscribe(value =>
                {
                    if (m_rigidbody != null)
                    {
                        m_rigidbody.linearVelocity = value;
                    }
                })
                .RegisterTo(this.destroyCancellationToken);
        }

        private void OnCollisionEnter(Collision collision)
        {
            m_viewModel?.Hit(collision);
            if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Floor"))
            {
                ContactPoint contact = collision.contacts[0];
                Vector3 normal = contact.normal;
                Transform decalRoot = GameObject.Find("DecalRoot")?.transform;

                if (decalRoot == null)
                {
                    Debug.LogError("DecalRoot object not found. Please create an empty GameObject named 'DecalRoot' in the scene hierarchy.");
                    return;
                }

                Vector3 decalPos = contact.point;
                Quaternion rotation = Quaternion.LookRotation(normal);

                decalPos += normal * 0.01f; 

                var decalInstance = Instantiate(decalPrefab, decalPos, rotation, decalRoot);
                    
                Destroy(decalInstance, decalLifeTime);
            }
            
        }

    }
}