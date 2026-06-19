#if UNITY_STANDALONE || UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VLCNP.Core
{
    public sealed class StandaloneAspectRatioController : MonoBehaviour
    {
        private const float TargetAspect = 16f / 9f;
        private const string ControllerName = "[StandaloneAspectRatioController]";
        private const string LetterboxCameraName = "[StandaloneLetterboxCamera]";

        private static StandaloneAspectRatioController instance;

        private Camera letterboxCamera;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (instance != null)
                return;

            GameObject controllerObject = new GameObject(ControllerName);
            DontDestroyOnLoad(controllerObject);
            instance = controllerObject.AddComponent<StandaloneAspectRatioController>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureFullscreenWindow();
            EnsureLetterboxCamera();
            ApplyViewport(force: true);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                instance = null;
            }
        }

        private void LateUpdate()
        {
            ApplyViewport(force: false);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyViewport(force: true);
        }

        private void EnsureFullscreenWindow()
        {
#if UNITY_STANDALONE && !UNITY_EDITOR
            Resolution resolution = Screen.currentResolution;
            if (!Screen.fullScreen || Screen.fullScreenMode != FullScreenMode.FullScreenWindow)
            {
                Screen.SetResolution(
                    resolution.width,
                    resolution.height,
                    FullScreenMode.FullScreenWindow
                );
            }
#endif
        }

        private void EnsureLetterboxCamera()
        {
            if (letterboxCamera != null)
                return;

            GameObject cameraObject = new GameObject(LetterboxCameraName);
            DontDestroyOnLoad(cameraObject);
            letterboxCamera = cameraObject.AddComponent<Camera>();
            letterboxCamera.clearFlags = CameraClearFlags.SolidColor;
            letterboxCamera.backgroundColor = Color.black;
            letterboxCamera.cullingMask = 0;
            letterboxCamera.depth = -10000f;
            letterboxCamera.orthographic = true;
            letterboxCamera.allowHDR = false;
            letterboxCamera.allowMSAA = false;
            letterboxCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }

        private void ApplyViewport(bool force)
        {
            EnsureLetterboxCamera();

            int screenWidth = Screen.width;
            int screenHeight = Screen.height;
            Camera[] cameras = Camera.allCameras;
            Rect viewport = CalculateViewport(screenWidth, screenHeight);

            for (int i = 0; i < cameras.Length; i++)
            {
                Camera targetCamera = cameras[i];
                if (!ShouldApplyViewport(targetCamera))
                    continue;

                if (force || !Approximately(targetCamera.rect, viewport))
                    targetCamera.rect = viewport;
            }
        }

        private static Rect CalculateViewport(int screenWidth, int screenHeight)
        {
            if (screenWidth <= 0 || screenHeight <= 0)
                return new Rect(0f, 0f, 1f, 1f);

            float screenAspect = (float)screenWidth / screenHeight;

            if (screenAspect > TargetAspect)
            {
                float width = TargetAspect / screenAspect;
                float x = (1f - width) * 0.5f;
                return new Rect(x, 0f, width, 1f);
            }

            float height = screenAspect / TargetAspect;
            float y = (1f - height) * 0.5f;
            return new Rect(0f, y, 1f, height);
        }

        private bool ShouldApplyViewport(Camera targetCamera)
        {
            if (targetCamera == null || targetCamera == letterboxCamera)
                return false;

            return targetCamera.cameraType == CameraType.Game;
        }

        private static bool Approximately(Rect left, Rect right)
        {
            return Mathf.Approximately(left.x, right.x)
                && Mathf.Approximately(left.y, right.y)
                && Mathf.Approximately(left.width, right.width)
                && Mathf.Approximately(left.height, right.height);
        }
    }
}
#endif
