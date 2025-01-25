using System;
using UnityEngine;
using System.Threading.Tasks;

namespace TheSTAR.GUI
{
    public abstract class GuiObject : MonoBehaviour
    {
        protected bool isShow = false;

        public bool IsShow => isShow;

        /// <summary>
        /// Init method to override. This method will call automatically
        /// </summary>
        /// <param name="cts">Available controllers to use</param>
        public virtual void Init(ControllerStorage cts) { }

        public virtual void Reset() { }

        #region Show/Hide

        public async void Show(Action endAction = null, bool skipShowAnim = false)
        {
            bool screenIsAlreadyOpen = isShow;

            isShow = true;
            gameObject.SetActive(true);

            OnShow();

            if (!skipShowAnim && !screenIsAlreadyOpen)
            {
                AnimateShow(out int hideTime);
                await Task.Delay(hideTime);
            }

            endAction?.Invoke();
        }

        public async void Hide(Action endAction = null)
        {
            AnimateHide(out int hideTime);

            await Task.Delay(hideTime);

            gameObject.SetActive(false);
            OnHide();
            isShow = false;

            endAction?.Invoke();
        }

        protected virtual void AnimateShow(out int showTime) => showTime = 0;

        protected virtual void AnimateHide(out int hideTime) => hideTime = 0;

        protected virtual void OnShow() { }

        protected virtual void OnHide() { }

        #endregion
    }
}