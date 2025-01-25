using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Play.Review;

public class IARManager : MonoBehaviour, IController
{
    private ReviewManager _reviewManager;
    private PlayReviewInfo _playReviewInfo;

    private static IARManager instance;
    public static IARManager Instance => instance;
    
    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void ShowInAppReview()
    {
        StartCoroutine(RequestReview());
    }

    private IEnumerator RequestReview()
    {
        Debug.Log("RequestReview");

        _reviewManager = new();
        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            yield break;
        }

        _playReviewInfo = requestFlowOperation.GetResult();

        // Launch the In App Review

        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;
        _playReviewInfo = null; // Reset the object
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            yield break;
        }
        // The flow has finished. The API does not indicate whether the user
        // reviewed or not, or even whether the review dialog was shown. Thus, no
        // matter the result, we continue our app flow.
    }

    #region Comparable

    public int CompareTo(IController other) => ToString().CompareTo(other.ToString());
    public int CompareToType<T>() where T : IController => ToString().CompareTo(typeof(T).ToString());

    #endregion
}