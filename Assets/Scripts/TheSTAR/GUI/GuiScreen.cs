using System;
using UnityEngine;
using TheSTAR.Utility;
using Sirenix.OdinInspector;

namespace TheSTAR.GUI
{
    public abstract class GuiScreen : GuiObject, IComparable<GuiScreen>, IComparableType<GuiScreen>
    {
        [SerializeField] private bool _rootScreen;
        [SerializeField] private bool _pause;
        [SerializeField] private bool _draggableCamera;

        [Space]
        [SerializeField] private bool _useCounters;
        [ShowIf("_useCounters")] [SerializeField] private bool _useSoftCounter;
        [ShowIf("_useCounters")] [SerializeField] private bool _useHardCounter;
        [ShowIf("_useCounters")] [SerializeField] private bool _useSoftPerSecondCounter;
        [ShowIf("_useCounters")] [SerializeField] private bool _useIncomeIncreaseCounter;

        public bool Root => _rootScreen;
        public bool Pause => _pause;
        public bool DraggableCamera => _draggableCamera;

        public bool UseCounters => _useCounters;
        public bool UseSoftCounter => _useSoftCounter;
        public bool UseHardCounter => _useHardCounter;
        public bool UseSoftPerSecondCounter => _useSoftPerSecondCounter;
        public bool UseIncomeIncreaseCounter => _useIncomeIncreaseCounter;

        protected const float DefaultAnimTime = 0.25f;

        public override string ToString() => GetType().ToString();

        public int CompareTo(GuiScreen other) => ToString().CompareTo(other.ToString());

        public int CompareToType<T>() where T : GuiScreen => ToString().CompareTo(typeof(T).ToString());
    }
}