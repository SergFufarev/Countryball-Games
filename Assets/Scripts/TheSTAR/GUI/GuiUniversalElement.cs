using System;
using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility;

namespace TheSTAR.GUI
{
    /// <summary>
    /// GuiObject for multiple screens (for example TopBar with currency counters)
    /// </summary>
    public abstract class GuiUniversalElement : GuiObject, IComparable<GuiUniversalElement>, IComparableType<GuiUniversalElement>
    {
        #region Comparable

        public override string ToString() => GetType().ToString();

        public int CompareTo(GuiUniversalElement other) => ToString().CompareTo(other.ToString());

        public int CompareToType<T>() where T : GuiUniversalElement => ToString().CompareTo(typeof(T).ToString());

        #endregion
    }
}