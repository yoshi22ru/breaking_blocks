using R3;
using System;

namespace Graffity.HandGesture.Sample.FingerGun
{
    public interface IRemainingShotsViewModel
    {
        ReadOnlyReactiveProperty<string> RemainingShotsText { get; }
    }

    public class RemainingShotsViewModel : IRemainingShotsViewModel, IDisposable
    {
        private readonly CompositeDisposable m_disposable = new();
        public ReadOnlyReactiveProperty<string> RemainingShotsText { get; }

        public RemainingShotsViewModel(SampleFingerGunModel model)
        {
            // 残弾数を文字列に変換して購読
            RemainingShotsText = model.RemainingShots
                .Select(shots => $"BULLET: {shots}")
                .ToReadOnlyReactiveProperty("BULLET: 30") // 初期値設定
                .AddTo(m_disposable);
        }

        public void Dispose()
        {
            if (!m_disposable.IsDisposed)
                m_disposable.Dispose();
        }
    }
}
