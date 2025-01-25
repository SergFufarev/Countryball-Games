using UnityEngine;
using Cinemachine;
using FunnyBlox;

namespace Battle
{
    public class BattleCameraController : MonoBehaviour, IController
    {
        [Space]
        [SerializeField] private CinemachineVirtualCamera putCamera;
        [SerializeField] private CinemachineVirtualCamera battleCamera;

        [Space]
        [SerializeField] private CameraZoomHelper zoomHelper;

        [Header("Modes")]
        [SerializeField] private float[] modeDistances;

        private float currentDistance;

        private int currentModeIndex = 0;

        public void Init()
        {
            /*
            if (PlayerPrefs.HasKey(CommonData.PREFSKEY_CURRENT_CAMERA_MODE_IN_BIG_BATTLE))
            {
                currentModeIndex = SaveManager.Load<int>(CommonData.PREFSKEY_CURRENT_CAMERA_MODE_IN_BIG_BATTLE);
            }
            else
            */
            currentModeIndex = 0;

            SetMode(currentModeIndex);
            zoomHelper.Init(modeDistances[currentModeIndex], OnChangeValueInZoomHelper);
        }

        private void OnChangeValueInZoomHelper(float value)
        {
            SetDistance(value);
        }

        public void SetMode(int modeIndex)
        {
            currentModeIndex = modeIndex;
            SetDistance(modeDistances[currentModeIndex]);
            SaveManager.Save(CommonData.PREFSKEY_CURRENT_CAMERA_MODE_IN_BIG_BATTLE, modeIndex);
        }

        public void SwitchMode()
        {
            int nearModeIndex = 0;
            float minDifference = Mathf.Abs(currentDistance - modeDistances[0]);

            for (int i = 1; i < modeDistances.Length; i++)
            {
                if (Mathf.Abs(currentDistance - modeDistances[i]) < minDifference) nearModeIndex = i;
            }

            currentModeIndex = nearModeIndex + 1;
            if (currentModeIndex >= modeDistances.Length) currentModeIndex = 0;
            SetMode(currentModeIndex);
        }

        public void SetVirtualCameraType(VirtualCameraType virtualCameraType)
        {
            putCamera.gameObject.SetActive(virtualCameraType == VirtualCameraType.Put);
            battleCamera.gameObject.SetActive(virtualCameraType == VirtualCameraType.Battle);
        }

        private void SetDistance(float distance)
        {
            putCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = distance;
            battleCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = distance;
            currentDistance = distance;
            zoomHelper.Set(distance);
        }

        #region Comparable

        public int CompareTo(IController other) => ToString().CompareTo(other.ToString());
        public int CompareToType<T>() where T : IController => ToString().CompareTo(typeof(T).ToString());

        #endregion
    }

    public enum VirtualCameraType
    {
        Put,
        Battle
    }
}