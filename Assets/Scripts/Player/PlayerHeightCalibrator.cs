using UnityEngine;
using Unity.XR.CoreUtils;

namespace SafetyTraining.Player
{
    /// <summary>
    /// Highly robust player perspective and height calibrator.
    /// Supports setting the viewpoint to an average Japanese male (1.60m eye height),
    /// average Japanese female (1.48m eye height), or custom height.
    /// Works seamlessly in both physical VR play mode and simulated/editor mode.
    /// </summary>
    [RequireComponent(typeof(XROrigin))]
    public class PlayerHeightCalibrator : MonoBehaviour
    {
        public enum PerspectivePreset
        {
            AverageMale,    // Standard Japanese male: standing height ~1.71m, eye height ~1.60m
            AverageFemale,  // Standard Japanese female: standing height ~1.58m, eye height ~1.48m
            Custom          // User-defined eye height
        }

        public enum CalibrationMode
        {
            ContinuousLock,     // Continuously lock eye height (perfect for simulation or seated VR play)
            StaticCalibration   // Calibrate once at start or on demand to preserve relative physical crouching
        }

        [Header("Height Settings")]
        [Tooltip("Select the perspective preset to apply.")]
        [SerializeField] private PerspectivePreset perspectivePreset = PerspectivePreset.AverageMale;

        [Tooltip("Specify a custom eye height (only used if preset is set to Custom).")]
        [SerializeField] private float customEyeHeight = 1.60f;

        [Tooltip("Calibration Mode. Continuous Lock always enforces the height. Static Calibration calibrates once to allow physical crouching.")]
        [SerializeField] private CalibrationMode calibrationMode = CalibrationMode.ContinuousLock;

        [Header("Advanced / Manual Calibration")]
        [Tooltip("Hotkey to manually trigger calibration during play.")]
        [SerializeField] private KeyCode recalibrateKey = KeyCode.C;

        [Tooltip("Log warning if tracking has not initialized yet.")]
        [SerializeField] private bool verboseLogging = true;

        private XROrigin xrOrigin;
        private Transform cameraOffset;
        private Transform xrCamera;
        private CharacterController characterController;

        private bool isCalibrated = false;
        private float lastCalibratedCameraY = 0f;

        // Standard biometric eye heights (meters)
        public const float AverageMaleEyeHeight = 1.60f;
        public const float AverageFemaleEyeHeight = 1.48f;

        /// <summary>
        /// Gets the target eye height based on the selected preset.
        /// </summary>
        public float TargetEyeHeight
        {
            get
            {
                switch (perspectivePreset)
                {
                    case PerspectivePreset.AverageMale:
                        return AverageMaleEyeHeight;
                    case PerspectivePreset.AverageFemale:
                        return AverageFemaleEyeHeight;
                    case PerspectivePreset.Custom:
                        return customEyeHeight;
                    default:
                        return AverageMaleEyeHeight;
                }
            }
        }

        private void Awake()
        {
            xrOrigin = GetComponent<XROrigin>();
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            if (xrOrigin == null)
            {
                Debug.LogError("[PlayerHeightCalibrator] XROrigin component is missing on this GameObject!", this);
                enabled = false;
                return;
            }

            // Cache references
            cameraOffset = xrOrigin.CameraFloorOffsetObject != null ? xrOrigin.CameraFloorOffsetObject.transform : null;
            xrCamera = xrOrigin.Camera != null ? xrOrigin.Camera.transform : null;

            if (cameraOffset == null || xrCamera == null)
            {
                // Fallback search in children if not explicitly assigned
                if (xrCamera == null)
                {
                    Camera mainCam = Camera.main;
                    if (mainCam != null) xrCamera = mainCam.transform;
                }

                if (cameraOffset == null && xrCamera != null)
                {
                    cameraOffset = xrCamera.parent;
                }

                if (cameraOffset == null || xrCamera == null)
                {
                    Debug.LogError("[PlayerHeightCalibrator] Camera Offset or Main Camera could not be resolved! Calibration disabled.", this);
                    enabled = false;
                    return;
                }
            }

            // Adjust character controller height to match the standing height of the preset
            AdjustCharacterController();
        }

        private void Update()
        {
            // Handle manual recalibration key
            if (Input.GetKeyDown(recalibrateKey))
            {
                Recalibrate();
            }

            if (calibrationMode == CalibrationMode.ContinuousLock)
            {
                ApplyContinuousLock();
            }
            else if (calibrationMode == CalibrationMode.StaticCalibration && !isCalibrated)
            {
                TryStaticCalibration();
            }
        }

        /// <summary>
        /// Adjusts the character controller's height and center to match the physical stature of the preset.
        /// </summary>
        private void AdjustCharacterController()
        {
            if (characterController == null) return;

            float standingHeight = 1.71f; // Default average male height
            if (perspectivePreset == PerspectivePreset.AverageFemale)
            {
                standingHeight = 1.58f;
            }
            else if (perspectivePreset == PerspectivePreset.Custom)
            {
                // Simple approximation of standing height from eye height (typically eye height is ~93-94% of standing height)
                standingHeight = customEyeHeight / 0.93f;
            }

            characterController.height = standingHeight;
            characterController.center = new Vector3(0f, standingHeight / 2f, 0f);

            if (verboseLogging)
            {
                Debug.Log($"[PlayerHeightCalibrator] CharacterController adjusted to height: {standingHeight:F2}m (center Y: {(standingHeight / 2f):F2}m)");
            }
        }

        /// <summary>
        /// Continuously forces the camera offset Y position so the absolute eye height is locked.
        /// </summary>
        private void ApplyContinuousLock()
        {
            float currentCameraLocalY = xrCamera.localPosition.y;

            // Wait for tracking to initialize if it reports 0
            if (currentCameraLocalY <= 0.01f)
            {
                // In Editor/Simulator, fallback to default CameraYOffset if tracking is not sending values yet
                currentCameraLocalY = xrOrigin.CameraYOffset;
                if (currentCameraLocalY <= 0.01f) currentCameraLocalY = TargetEyeHeight;
            }

            Vector3 offsetLocalPos = cameraOffset.localPosition;
            float targetOffsetY = TargetEyeHeight - currentCameraLocalY;

            // Apply target offset
            cameraOffset.localPosition = new Vector3(offsetLocalPos.x, targetOffsetY, offsetLocalPos.z);
        }

        /// <summary>
        /// Attempts to calibrate once at start when valid tracking data is received.
        /// </summary>
        private void TryStaticCalibration()
        {
            float currentCameraLocalY = xrCamera.localPosition.y;

            // We require a valid tracking height (greater than 0.2m) to calibrate statically.
            // If the user hasn't put on the headset or simulator is loading, we wait.
            if (currentCameraLocalY > 0.2f)
            {
                Vector3 offsetLocalPos = cameraOffset.localPosition;
                float targetOffsetY = TargetEyeHeight - currentCameraLocalY;
                cameraOffset.localPosition = new Vector3(offsetLocalPos.x, targetOffsetY, offsetLocalPos.z);

                isCalibrated = true;
                lastCalibratedCameraY = currentCameraLocalY;

                if (verboseLogging)
                {
                    Debug.Log($"[PlayerHeightCalibrator] Static Calibration Complete! Stand eye height: {currentCameraLocalY:F2}m. Camera Offset adjusted to: {targetOffsetY:F2}m.");
                }
            }
        }

        /// <summary>
        /// Manually triggers a recalibration.
        /// </summary>
        public void Recalibrate()
        {
            isCalibrated = false;
            AdjustCharacterController();
            
            if (calibrationMode == CalibrationMode.ContinuousLock)
            {
                ApplyContinuousLock();
                if (verboseLogging) Debug.Log("[PlayerHeightCalibrator] Continuous height lock re-applied.");
            }
            else
            {
                TryStaticCalibration();
            }
        }

        /// <summary>
        /// Set a new preset at runtime.
        /// </summary>
        public void SetPreset(PerspectivePreset preset)
        {
            perspectivePreset = preset;
            Recalibrate();
        }
    }
}
