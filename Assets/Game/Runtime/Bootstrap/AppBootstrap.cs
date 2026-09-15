using UnityEngine;
using BubbleShot.Runtime.Lifecycle;

namespace BubbleShot.Runtime.Bootstrap
{
    /// <summary>
    /// Application entry point attached to the persistent root in Bootstrap.unity.
    /// Initializes core runtime services and routes to MainMenu.
    /// </summary>
    public class AppBootstrap : MonoBehaviour
    {
        [SerializeField] private bool _autoLoadMainMenu = true;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeApp();
        }

        private void Start()
        {
            if (_autoLoadMainMenu)
            {
                SceneNavigator.LoadScene(GameScene.MainMenu);
            }
        }

        private void InitializeApp()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            GameLogger.LogInfo("AppBootstrap", "Application initialized with target 60 FPS.");
        }
    }
}
