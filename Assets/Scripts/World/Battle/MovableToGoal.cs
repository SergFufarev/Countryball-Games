using System;
using System.Collections.Generic;
using UnityEngine;

public class MovableToGoal
{
    private MoveToGoalMode _mode;
    private float _neededGoalDistance = 0.25f;
    private Transform _transform;
    private Action _onGoalReached;
    private float _speed = 1;
    private bool _goalWasReached = false;
    private bool _attachAfterReach = false; // attach object position to goal after reach (you can move goal-object and this object will moving too)
    private Vector3 _attachOffset;

    public MovableToGoal(MoveToGoalMode mode, Transform transform, float speed, float neededGoalDistance, Action onGoalReached, bool attachAfterReach = false)
    {
        _mode = mode;
        _transform = transform;
        _speed = speed;
        _neededGoalDistance = neededGoalDistance;
        _onGoalReached = onGoalReached;
        _attachAfterReach = attachAfterReach;
    }

    public void MoveTo(Transform goal)
    {
        float distance = GetDistanceForCurrentMode(goal);
        if (distance <= _neededGoalDistance)
        {
            ReachGoal();
            return;
        }

        Vector3 step;
        var direction = _mode switch
        {
            MoveToGoalMode.OnlyX => new Vector3(1, 0, 0),
            MoveToGoalMode.OnlyY => new Vector3(0, 1, 0),
            MoveToGoalMode.OnlyZ => new Vector3(0, 0, 1),
            _ => (goal.transform.position - _transform.position).normalized,
        };

        step = _speed * Time.deltaTime * direction;

        if (Vector3.Distance(Vector3.zero, step) > distance)
        {
            ReachGoal();
            return;
        }
        else _transform.Translate(step);

        float GetDistanceForCurrentMode(Transform goal)
        {
            float distance = 0;
            switch (_mode)
            {
                case MoveToGoalMode.Full3D:
                    distance = Vector3.Distance(_transform.position, goal.position);
                    break;

                case MoveToGoalMode.OnlyX:
                    distance = Math.Abs(_transform.position.x - goal.position.x);
                    break;

                case MoveToGoalMode.OnlyY:
                    distance = Math.Abs(_transform.position.y - goal.position.y);
                    break;

                case MoveToGoalMode.OnlyZ:
                    distance = Math.Abs(_transform.position.z - goal.position.z);
                    break;
            }

            return distance;
        }

        void ReachGoal()
        {
            if (!_goalWasReached)
            {
                _onGoalReached?.Invoke();
                _goalWasReached = true;

                if (_attachAfterReach) _attachOffset = _transform.position - goal.transform.position;
            }
            else
            {
                if (_attachAfterReach) _transform.position = goal.transform.position + _attachOffset;
            }
        }
    }

    public void ResetReached() => _goalWasReached = false;
}

public enum MoveToGoalMode
{
    /// <summary> Use all directions (XYZ) </summary>
    Full3D,

    /// <summary> Use only X direction </summary>
    OnlyX,

    /// <summary> Use only Y direction </summary>
    OnlyY,

    /// <summary> Use only Z direction </summary>
    OnlyZ
}