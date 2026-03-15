using UnityEngine;

namespace Game.UI
{
    public class SettingsPanelController : MonoBehaviour
    {
        [SerializeField] private bool pauseGameWhenVisible;

        private void OnEnable()
        {
            if (pauseGameWhenVisible)
            {
                Time.timeScale = 0f;
            }
        }

        private void OnDisable()
        {
            if (pauseGameWhenVisible)
            {
                Time.timeScale = 1f;
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }
    }
}