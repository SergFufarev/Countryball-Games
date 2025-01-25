using System;
using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility;

public class ControllerStorage
{
    public IController[] controllers;

    public ControllerStorage(IController[] controllers) => this.controllers = controllers;

    public T Get<T>() where T : class, IController
    {
        for (int i = 0; i < controllers.Length; i++) if (controllers[i] is T t) return t;
        return null;
    }
}

public interface IController : IComparable<IController>, IComparableType<IController>
{}