using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hara.GUI
{
    public class PopupManager : MonoBehaviour
    {
        static PopupManager _instance = null;
        [HideInInspector] public GraphicRaycaster graphicRaycasterPopupManager;
        public static PopupManager instance
        {
            get
            {
                if (_instance == null)
                    _instance = GameObject.Find("PopupContainer").GetComponent<PopupManager>();
                return _instance;
            }
        }

        [System.NonSerialized]
        public List<PopupBase> activePopups = new List<PopupBase>();
        Stack<PopupBase> backStackPopups = new Stack<PopupBase>();

        private static Vector3 POPUP_DEFAULT_POSITION = Vector3.zero;
        private static Dictionary<string, GameObject> popupsPref = new Dictionary<string, GameObject>();
        private static Dictionary<string, GameObject> popupsCache = new Dictionary<string, GameObject>();

 

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            this.ClearAll();
        }

        private void Start()
        {
            graphicRaycasterPopupManager = GetComponent<GraphicRaycaster>();
        }

        public void ClearAll()
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                if (this.transform.GetChild(i) != null)
                {
                    Destroy(this.transform.GetChild(i).gameObject);
                }
            }

            this.activePopups.Clear();
        }

        public bool HaveBackStackPopup()
        {
            return backStackPopups.Count > 0;
        }

        public void OnBackBtnClick()
        {
            if (HaveBackStackPopup())
            {
                var popup = backStackPopups.Peek();
                popup.OnBackBtnClick();
            }
        }

        public void OnShowPopup(PopupBase popup)
        {
            if (this.activePopups.Contains(popup) == false) this.activePopups.Add(popup);
            if (popup.addToBackStack) backStackPopups.Push(popup);
        }

        public void OnHidePopup(PopupBase popup)
        {
            if (popup == null) return;
            int index = this.activePopups.FindIndex(v => v == popup);

            this.activePopups.RemoveAt(index);

            if (backStackPopups.Contains(popup))
            {
                var tempList = HoaiAnh.Util.ListPool<PopupBase>.Claim();
                var peek = backStackPopups.Count > 0 ? backStackPopups.Peek() : null;
                while (backStackPopups.Count > 0 && peek != popup)
                {
                    tempList.Add(backStackPopups.Pop());
                    peek = backStackPopups.Count > 0 ? backStackPopups.Peek() : null;
                }

                if (peek == popup)
                {
                    backStackPopups.Pop();
                }

                for (int i = tempList.Count - 1; i >= 0; --i)
                {
                    backStackPopups.Push(tempList[i]);
                }
                HoaiAnh.Util.ListPool<PopupBase>.Release(tempList);
            }
        }

        public void OnCreatePopup(PopupBase popup)
        {
            if (popup == null) return;
        }

        private GameObject GetPopupPref(string popupName)
        {
            GameObject pref = null;
            if (!popupsPref.ContainsKey(popupName))
            {
                pref = Resources.Load("Popups/" + popupName) as GameObject;
                popupsPref[popupName] = pref;
            }
            else
            {
                pref = popupsPref[popupName];
                if (pref == null)
                {
                    popupsPref.Remove(popupName);
                    this.GetPopupPref(popupName);
                }
            }
            return pref;
        }

        public GameObject GetPopup(string popupName, bool useDefaultPos = true, Vector3 pos = default(Vector3), bool isMultiInstance = false)
        {
            GameObject obj = null;
            string name = popupName + "(Clone)";
            if (!popupsCache.ContainsKey(name) || isMultiInstance)
            {
                GameObject pref = this.GetPopupPref(popupName);
                obj = Instantiate(pref, transform);
            }
            else
            {
                obj = popupsCache[name];
                if (obj == null)
                {
                    popupsCache.Remove(name);
                    this.GetPopup(popupName, useDefaultPos, pos);
                }
            }
            obj.SetActive(true);
            if (useDefaultPos) pos = POPUP_DEFAULT_POSITION;
            if (this.activePopups.Count > 1)
            {
                //pos.z = this.activePopups[this.activePopups.Count - 2].transform.localPosition.z - 700;
            }
            obj.transform.localPosition = pos;

            return obj;
        }
    }
}