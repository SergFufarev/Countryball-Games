using System;
using Sirenix.OdinInspector;
using TheSTAR.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace TheSTAR.Utility.Pointer
{
    public class PointerButton : Pointer
    {
        [SerializeField] protected PointerButtonInfo _info = new PointerButtonInfo();

        protected virtual PointerButtonInfo currentInfo => _info;

        public Image Img => currentInfo._img;

        private Action _clickAction;
        private Action _enterAction;
        private Action _exitAction;
        private SoundController _soundController;
        private bool _isEnter = false;
        private bool _isDown = false;
        private bool _isInteractalbe = true;

        private void Start()
        {
            InitPointer(
                (pointer) => OnButtonDown(),
                null,
                (pointer) => OnButtonUp(),
                (pointer) => OnButtonEnter(),
                (pointer) => OnButtonExit());
        }

        /*
        public void Init(SoundController soundController)
        {
            if (soundController != null) _soundController = soundController;
        }
        */

        public void Init(Action clickAction, SoundController soundController = null, Action enterAction = null, Action exitAction = null)
        {
            _clickAction = clickAction;

            if (_soundController == null) _soundController = SoundController.Instance;

            _enterAction = enterAction;
            _exitAction = exitAction;
        }

        private void OnButtonEnter()
        {
            if (!_isInteractalbe) return;
            if (currentInfo._useEnterSound && _soundController) _soundController.PlaySound(currentInfo._enterSoundType);

            _isEnter = true;

            UpdateVisual();

            _enterAction?.Invoke();
        }

        private void OnButtonExit()
        {
            if (!_isInteractalbe) return;

            _isEnter = false;

            UpdateVisual();

            _exitAction?.Invoke();
        }

        private void OnButtonDown()
        {
            if (!_isInteractalbe) return;
            if (currentInfo._useClickSound && _soundController != null)
            {
                _soundController.PlaySound(currentInfo._clickSoundType);
            }

            _isDown = true;

            UpdateVisual();
        }

        private void OnButtonUp()
        {
            if (!_isInteractalbe) return;
            if (_isEnter) _clickAction?.Invoke();

            _isDown = false;

            UpdateVisual();
        }

        private void OnDisable()
        {
            _isEnter = false;
            _isDown = false;
            UpdateVisual();
        }

        public void SetInteractalbe(bool value)
        {
            _isInteractalbe = value;
            UpdateVisual();
        }

        protected void UpdateVisual()
        {
            try
            {
                if (!_isInteractalbe)
                {
                    if (currentInfo._useChangeColor) currentInfo._img.color = currentInfo._disableColor;
                    if (currentInfo._useChangeSprite) currentInfo._img.sprite = currentInfo._disableSprite;
                }
                else if (_isEnter)
                {
                    if (currentInfo._useChangeColor) currentInfo._img.color = _isDown ? currentInfo._pressColor : currentInfo._selectColor;
                    if (currentInfo._useChangeSprite) currentInfo._img.sprite = _isDown ? currentInfo._pressSprite : currentInfo._selectSprite;
                }
                else
                {
                    if (currentInfo._useChangeColor) currentInfo._img.color = currentInfo._idleColor;
                    if (currentInfo._useChangeSprite) currentInfo._img.sprite = currentInfo._idleSprite;
                }
            }
            catch
            {
                Debug.LogError("Error with PointerButton: " + gameObject.name, gameObject);
            }
        }

        [Serializable]
        public class PointerButtonInfo
        {
            public bool _useEnterSound;
            [ShowIf("_useEnterSound")] public SoundType _enterSoundType;

            public bool _useClickSound;
            [ShowIf("_useClickSound")] public SoundType _clickSoundType;

            public bool _useChangeSprite;
            [ShowIf("_useChangeSprite")] public Sprite _idleSprite;
            [ShowIf("_useChangeSprite")] public Sprite _selectSprite;
            [ShowIf("_useChangeSprite")] public Sprite _pressSprite;
            [ShowIf("_useChangeSprite")] public Sprite _disableSprite;

            public bool _useChangeColor;
            [ShowIf("_useChangeColor")] public Color _idleColor = Color.white;
            [ShowIf("_useChangeColor")] public Color _selectColor = Color.white;
            [ShowIf("_useChangeColor")] public Color _pressColor = new (0.7f, 0.7f, 0.7f);
            [ShowIf("_useChangeColor")] public Color _disableColor = Color.white;

            [ShowIf("@_useChangeSprite || _useChangeColor")] public Image _img;
        }
    }
}