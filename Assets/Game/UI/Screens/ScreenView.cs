using UnityEngine;

namespace BubbleShot.UI.Screens
{
    /// <summary>
    /// Base class for all managed canvas UI screens.
    /// </summary>
    public abstract class ScreenView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup? _canvasGroup;

        public virtual void Show()
        {
            gameObject.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        public virtual void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
        }
    }
}
