using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;

public static class SpineHelper
{
    public static float GetAnimationDuration(SkeletonGraphic skeleton, string animationName)
    {
        return skeleton.Skeleton.Data.FindAnimation(animationName)?.Duration ?? 0;
    }

    public static float GetAnimationDuration(SkeletonAnimation skeleton, string animationName)
    {
        return skeleton.Skeleton.Data.FindAnimation(animationName)?.Duration ?? 0;
    }

    public static void PlayAnimation(SkeletonGraphic skeleton, string animationName, bool loop)
    {
        skeleton.AnimationState.SetAnimation(0, animationName, loop);
    }

    public static void PlayAnimation(SkeletonAnimation skeleton, string animationName, bool loop)
    {
        skeleton.AnimationState.SetAnimation(0, animationName, loop);
    }
    public static string GetCurrentAnimationName(SkeletonGraphic skeleton)
    {
        TrackEntry currentTrackEntry = skeleton.AnimationState.GetCurrent(0);
        if (currentTrackEntry != null)
        {
            return currentTrackEntry.Animation.Name;
        }
        else
        {
            return null;
        }
    }

    public static string GetCurrentAnimationName(SkeletonAnimation skeleton)
    {
        TrackEntry currentTrackEntry = skeleton.AnimationState.GetCurrent(0);
        if (currentTrackEntry != null)
        {
            return currentTrackEntry.Animation.Name;
        }
        else
        {
            return null;
        }
    }

    public static void FadeSkeleton(SkeletonAnimation skeleton, float targetValue = 0, float delay = 0.5f)
    {
        if (targetValue == 1)
        {
            skeleton.Skeleton.A = 0;
        }
        DOTween.To(() => skeleton.Skeleton.A, x => skeleton.Skeleton.A = x, targetValue, delay);
    }
    public static void FadeSkeleton(SkeletonGraphic skeleton, float targetValue = 0, float delay = 0.5f)
    {
        Color oldColor = skeleton.color;
        float alpha = oldColor.a;

        DOTween.To(() => alpha, x =>
        {
            alpha = x;
            skeleton.color = new Color(oldColor.r, oldColor.g, oldColor.b, alpha);
        }, targetValue, delay);
    }

    public static void ResetSkeletonAlpha(SkeletonAnimation skeleton)
    {
        skeleton.Skeleton.A = 1;
    }

    public static void ResetSkeletonAlpha(SkeletonGraphic skeleton)
    {
        skeleton.color = new Color(1, 1, 1, 1);
    }
}
