using UnityEngine;

namespace TheSTAR.Utility.Pointer
{
    public class PointerButtonMultiInfo : PointerButton
    {
        [SerializeField] private PointerButtonInfo[] _additionalInfos = new PointerButtonInfo[0];

        private bool _useAdditionalInfo = false;
        public bool useAdditionalInfo => _useAdditionalInfo;
        private sbyte _additionalInfoIndex = 0;

        protected override PointerButtonInfo currentInfo
        {
            get
            {
                if (_useAdditionalInfo) return _additionalInfos[_additionalInfoIndex];
                return _info;
            }
        }

        public void SetInfo(bool useAdditionalInfo)
        {
            SetInfo(useAdditionalInfo, _additionalInfoIndex);
        }

        public void SetInfo(bool useAdditionalInfo, sbyte additionalInfoIndex)
        {
            _useAdditionalInfo = useAdditionalInfo;
            _additionalInfoIndex = additionalInfoIndex;

            UpdateVisual();
        }
    }
}