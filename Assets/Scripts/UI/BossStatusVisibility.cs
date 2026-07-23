using UnityEngine;

namespace VLCNP.UI
{
    /**
     * 非アクティブ状態を含めて BossStatus の表示を切り替える
     */
    public static class BossStatusVisibility
    {
        private const string BossStatusTag = "BossStatus";

        public static bool SetVisible(bool isVisible)
        {
            GameObject bossStatus = FindInLoadedScene();
            if (bossStatus == null)
                return false;

            bossStatus.SetActive(isVisible);
            return true;
        }

        private static GameObject FindInLoadedScene()
        {
            GameObject[] gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject candidate in gameObjects)
            {
                if (candidate == null)
                    continue;
                if (!candidate.scene.IsValid() || !candidate.scene.isLoaded)
                    continue;
                if (!candidate.CompareTag(BossStatusTag))
                    continue;

                return candidate;
            }

            return null;
        }
    }
}
