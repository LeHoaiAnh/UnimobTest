using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hara.GUI
{
    public static class HikerGUIExtensions
    {
        static public void DestroyChildren(this Transform t)
        {
            bool isPlaying = Application.isPlaying;

            while (t.childCount != 0)
            {
                Transform child = t.GetChild(0);

                if (isPlaying)
                {
                    //child.parent = null;
                    //var rectTrans = child.GetComponent<RectTransform>();
                    //if (rectTrans)
                    //{
                    //    Object.Destroy(rectTrans);
                    //}
                    child.SetParent(null);
                    UnityEngine.Object.Destroy(child.gameObject);
                }
                else UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }
    }
}
