using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Hara.GUI
{
    public class PopupBase : MonoBehaviour
    {
        public bool addToBackStack = true;
        public bool hideIsDestroy = true;
        public UnityEngine.Events.UnityEvent onPopupClosed;
        protected GUIManager _guiManager;

        protected virtual void Awake()
        {
            _guiManager = GUIManager.Instance;
            PopupManager.instance.OnCreatePopup(this);
        }

        protected virtual void OnEnable()
        {
            PopupManager.instance.OnShowPopup(this);
        }

        protected virtual void OnDisable()
        {
            PopupManager.instance.OnHidePopup(this);
            onPopupClosed?.Invoke();
        }

        public void MakePopupToTop()
        {
            transform.SetAsLastSibling();
        }

        [GUIDelegate]
        protected virtual void OnCleanUp()
        {
        }

        [GUIDelegate]
        public virtual void OnBackBtnClick()
        {
            if (addToBackStack)
            {
                OnCloseBtnClick();
            }
        }

        [GUIDelegate]
        public virtual void OnCloseBtnClick()
        {
            OnCleanUp();
            Hide();
        }
        
        [GUIDelegate]
        protected virtual void OnCloseBtnClick(bool playSound = true)
        {
            if (playSound)
            {
                _guiManager.PlaySound("button_click");
            }

            OnCloseBtnClick();
        }

        protected void Hide()
        {
            if (gameObject)
            {
                if (hideIsDestroy)
                {
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}