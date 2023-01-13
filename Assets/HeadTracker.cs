using Assets.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadTracker : MonoBehaviour
{
    private bool stop = false;
    public Transform HeadTransform;
    private List<Vector> trackedHeadTransform = new List<Vector>();

    public void StartTracking()
    {
        stop = false;
        trackedHeadTransform = new List<Vector>();
        StartCoroutine(TrackCoroutine());
    }

    IEnumerator TrackCoroutine()
    {
        while (!stop)
        {
            trackedHeadTransform.Add(new Vector(HeadTransform.localEulerAngles));
            yield return new WaitForSecondsRealtime(1);
        }
    }

    public List<Vector> StopTracking()
    {
        stop = true;
        return trackedHeadTransform;
    }
}