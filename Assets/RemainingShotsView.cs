using R3;
using UnityEngine;
using TMPro; // Text Mesh Proを使用していることを前提とします

namespace Graffity.HandGesture.Sample.FingerGun
{
    public class RemainingShotsView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_remainingShotsText;
        private IRemainingShotsViewModel m_viewModel;
        private readonly CompositeDisposable m_viewDisposable = new();

        public void Bind(IRemainingShotsViewModel vm)
        {
            // 既存の購読を解除
            m_viewDisposable.Clear();
            m_viewModel = vm;
            
            if (m_remainingShotsText == null)
            {
                Debug.LogError($"{nameof(m_remainingShotsText)}がインスペクターで設定されていません。", this);
                return;
            }

            // ViewModelのテキストを購読し、UIに反映
            vm.RemainingShotsText
                .Subscribe(text => m_remainingShotsText.text = text)
                .AddTo(m_viewDisposable); // ← ✅ R3ではこちらを使う
        }

        private void OnDestroy()
        {
            m_viewDisposable.Dispose();
        }
    }
}
