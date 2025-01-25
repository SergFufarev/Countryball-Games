using System;
using UnityEngine;

namespace TheSTAR.Utility
{
    public static class AnimationUtility
    {
        public static int GetClipLength(Animation animator, string clipName)
        {
            var neededClip = animator.GetClip(clipName);
            int length = (int)(neededClip.length * 1000);

            return length;
        }

        public static int GetClipLength(Animator animator, string clipName)
        {
            var animController = animator.runtimeAnimatorController;
            var clips = animController.animationClips;
            var neededClip = Array.Find<AnimationClip>(clips, info => info.name == clipName);
            int length = (int)(neededClip.length * 1000);

            return length;
        }
    }
}