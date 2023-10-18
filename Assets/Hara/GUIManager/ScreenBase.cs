using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hara.GUI
{
    public class ScreenBase : MonoBehaviour
    {
        public virtual void OnActive()
        {

        }
        public virtual void OnDeactive()
        {

        }

        public virtual bool OnBackBtnClick()
        {
            return false;
        }
    }
}