using UnityEngine;
using UnityEngine.SceneManagement;

namespace BubbleShot.Runtime.Lifecycle
{
    public enum GameScene
    {
        Bootstrap = 0,
        MainMenu = 1,
        Gameplay = 2
    }

    /// <summary>
    /// Centralized scene transition controller ensuring safe asynchronous loading.
    /// </summary>
    public static class SceneNavigator
    {
        public const string BootstrapSceneName = "Bootstrap";
        public const string MainMenuSceneName = "MainMenu";
        public const string GameplaySceneName = "Gameplay";

        public static string GetSceneName(GameScene scene)
        {
            return scene switch
            {
                GameScene.Bootstrap => BootstrapSceneName,
                GameScene.MainMenu => MainMenuSceneName,
                GameScene.Gameplay => GameplaySceneName,
                _ => MainMenuSceneName
            };
        }

        public static void LoadScene(GameScene scene)
        {
            string sceneName = GetSceneName(scene);
            GameLogger.LogInfo("SceneNavigator", $"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        public static AsyncOperation LoadSceneAsync(GameScene scene)
        {
            string sceneName = GetSceneName(scene);
            GameLogger.LogInfo("SceneNavigator", $"Loading scene asynchronously: {sceneName}");
            return SceneManager.LoadSceneAsync(sceneName);
        }
    }
}
