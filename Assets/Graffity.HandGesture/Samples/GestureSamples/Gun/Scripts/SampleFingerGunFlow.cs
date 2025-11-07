using System;
using System.Collections.Generic;
using R3;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Hands;

// BulletModel.cs で定義された HandInfo, HandType などにアクセスするために必要
using Graffity.HandGesture.Sample.FingerGun; 
using static Graffity.HandGesture.Sample.FingerGun.BulletModel; 


namespace Graffity.HandGesture.Sample.FingerGun
{
    // ★注意: 以下の外部クラスは、プロジェクト内の別のファイルに定義されている必要があります
    // GestureManager, SampleFingerGunModel, BulletViewModel, TargetModel, TargetViewModel, StateWindowView, StateWindowViewModel, RemainingShotsView, RemainingShotsViewModel, BulletView, TargetView, IDisposable
    
    // 外部ライブラリの欠損エラーを避けるため、ここではクラス定義を維持します。
    // 実際のエラー解消には、これらの外部クラスのファイルが存在する必要があります。
    
    
    /// <summary>
    /// FingerGun Sample Flow (メインゲームロジック)
    /// </summary>
    public class SampleFingerGunFlow : MonoBehaviour
    {
        // ★警告: 以下のフィールドの型は外部ファイルに定義されている必要があります。
        [field: SerializeField]
        GestureManager GestureManager { get; set; }
        [field: SerializeField]
        Transform BulletRoot { get; set; }
        [field: SerializeField]
        GameObject BulletViewPrefab { get; set; }
        [field: SerializeField]
        Transform TargetRoot { get; set; }
        [field: SerializeField]
        GameObject TargetViewPrefab { get; set; }
        [field: SerializeField]
        StateWindowView StateWindowView_L { get; set; }
        [field: SerializeField]
        StateWindowView StateWindowView_R { get; set; }
        [field: SerializeField]
        RemainingShotsView RemainingShotsView { get; set; }

        SampleFingerGunModel Model { get; set; }
        List<BulletViewModel> BulletViewModelList { get; set; } = new();
        List<TargetViewModel> TargetViewModelList { get; set; } = new();
        StateWindowViewModel StateWindowViewModel_L { get; set; }
        StateWindowViewModel StateWindowViewModel_R { get; set; }
        RemainingShotsViewModel RemainingShotsViewModel { get; set; }


        void Start()
        {
            // Instantiate SampleFingerGunModel
            // ★注意: SampleFingerGunModel が定義されていることが前提
            Model = new SampleFingerGunModel(); 
            Model.RegisterTo(this.destroyCancellationToken); 
            

            // BulletViewModelとBulletViewを生成
            foreach (var bulletModel in Model.BulletModelList)
            {
                var bulletViewModel = new BulletViewModel();
                bulletViewModel.RegisterTo(this.destroyCancellationToken);
                bulletViewModel.Bind(bulletModel);
                // ShotPosition.CurrentValue は ReadOnlyReactiveProperty<Vector3> のため、CurrentValueで値を取得
                var initialPosition = bulletViewModel.ShotPosition.CurrentValue; 
                var bulletObject = Instantiate(BulletViewPrefab, initialPosition, Quaternion.identity, BulletRoot);

                if (bulletObject.TryGetComponent<BulletView>(out var bulletView))
                {
                    bulletView.Bind(bulletViewModel);
                }
            }   

            foreach (TargetModel targetModel in Model.TargetModelList)
            {
                var targetViewModel = new TargetViewModel();
                targetViewModel.RegisterTo(this.destroyCancellationToken);
                targetViewModel.Bind(targetModel);
                TargetViewModelList.Add(targetViewModel);
                var targetObject = Instantiate(TargetViewPrefab, TargetRoot);
                if (targetObject.TryGetComponent<TargetView>(out var targetView))
                {
                    Debug.Log("DEBUG: TargetView component found. Calling Bind."); 
                    targetView.Bind(targetViewModel);
                }
            }
    
            // RemainingShotsViewModelの生成とViewへのバインド
            RemainingShotsViewModel = new RemainingShotsViewModel(Model);
            RemainingShotsViewModel.RegisterTo(this.destroyCancellationToken);
            
            if (RemainingShotsView != null)
            {
                RemainingShotsView.Bind(RemainingShotsViewModel);
            }
            
            // input
            if (GestureManager.TryGetGesture("Left_FingerGun", out var l_gesture))
            {
                l_gesture.OnChangeResult()
                    .Where(value => value.result)
                    .DistinctUntilChanged() 
                    .Where(_ => Model.IsAnyBulletReadyToShot())
                    .Subscribe(_ => Model.Shot(GestureManager.LeftHandInfo))
                    .RegisterTo(this.destroyCancellationToken);
                // StateWindowViewModelの初期化は外部の型に依存
                StateWindowViewModel_L = new StateWindowViewModel(true); 
                StateWindowViewModel_L.RegisterTo(this.destroyCancellationToken);
                StateWindowViewModel_L.Bind(l_gesture);
                StateWindowView_L.Bind(StateWindowViewModel_L);
            }
            if (GestureManager.TryGetGesture("Right_FingerGun", out var r_gesture))
            {
                r_gesture.OnChangeResult()
                    .Where(value => value.result)
                    .DistinctUntilChanged() 
                    .Where(_ => Model.IsAnyBulletReadyToShot())
                    .Subscribe(_ => Model.Shot(GestureManager.RightHandInfo))
                    .RegisterTo(this.destroyCancellationToken);
                // StateWindowViewModelの初期化は外部の型に依存
                StateWindowViewModel_R = new StateWindowViewModel(false); 
                StateWindowViewModel_R.RegisterTo(this.destroyCancellationToken);
                StateWindowViewModel_R.Bind(r_gesture);
                StateWindowView_R.Bind(StateWindowViewModel_R);
            }
        }


        void Update()
        {
            Model.Update();
        }


    }


}