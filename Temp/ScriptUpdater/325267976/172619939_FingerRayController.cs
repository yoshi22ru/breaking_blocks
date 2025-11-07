using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Hands;

using System.Collections.Generic;
using System.Linq;
using Graffity.HandGesture;
using Graffity.HandGesture.Sample.FingerGun;

public class FingerRayController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BulletModel bulletModel;
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual lineVisual;

    [Header("Reticle & Shoot Settings")]
    [SerializeField] private Transform reticleTransform;
    [SerializeField] private float reticleDetectionRadius = 0.5f;
    [SerializeField] private InputActionProperty shootAction;

    private XRHandSubsystem m_handSubsystem;
    private bool m_isRayInsideReticle = false;

    void Start()
    {
        if (bulletModel == null || rayInteractor == null || lineVisual == null || reticleTransform == null || shootAction.action == null)
        {
            Debug.LogError("FingerRayController: 依存関係が未設定です。", this);
            enabled = false;
            return;
        }

        m_handSubsystem = GetHandSubsystem();
        SetRayVisible(false);

        shootAction.action.Enable();
        shootAction.action.performed += OnShootPerformed;
    }

    void OnDestroy()
    {
        shootAction.action.performed -= OnShootPerformed;
        shootAction.action.Disable();
    }

    void Update()
    {
        float distanceToReticle = Vector3.Distance(transform.position, reticleTransform.position);
        m_isRayInsideReticle = distanceToReticle <= reticleDetectionRadius;
        SetRayVisible(m_isRayInsideReticle);
    }

    private void SetRayVisible(bool isVisible)
    {
        if (lineVisual.enabled != isVisible)
        {
            lineVisual.enabled = isVisible;
            rayInteractor.enabled = isVisible;
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        if (!m_isRayInsideReticle) return;
        if (m_handSubsystem == null || !m_handSubsystem.running) return;

        var rightHand = m_handSubsystem.rightHand;
        if (!rightHand.isTracked) return;

        // Graffity.HandGesture.HandInfo を使って弾を撃つ
        HandInfo info = new HandInfo(HandInfo.HandType.Right);
        bulletModel.Shot(info);
    }

    private XRHandSubsystem GetHandSubsystem()
    {
        List<XRHandSubsystem> subsystems = new();
        SubsystemManager.GetSubsystems(subsystems);
        return subsystems.FirstOrDefault();
    }
}
