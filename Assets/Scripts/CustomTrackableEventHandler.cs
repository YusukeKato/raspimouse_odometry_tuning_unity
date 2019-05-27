using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class CustomTrackableEventHandler : DefaultTrackableEventHandler
{
    public bool isFound = false;
    public GameObject ImageTarget;
    public GameObject ParentObject;
    public GameObject MarkerPrefab;
    public GameObject rosConnector;

    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();

        bool isStart = false;
        isStart = rosConnector.GetComponent<OdometryTuner>().isTuning;
        if (isStart == true && isFound == false)
        {
            isFound = true;
            ImageTarget.transform.parent = ParentObject.transform;
            ParentObject.transform.position = ImageTarget.transform.position;
            ParentObject.transform.rotation = ImageTarget.transform.rotation;
        }
    }
}
