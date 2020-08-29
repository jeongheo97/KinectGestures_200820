using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Windows.Kinect;

public class KinectManager : MonoBehaviour
{
    public GameObject Player;
    private Turning turnScript;
    // 키넥트 파트
    private KinectSensor kinectSensor;
    private BodyFrameReader bodyFrameReader;
    //array for body data
    private int bodyCount;
    private Body[] bodies;

    private string leanLeftGestureName = "Lean_Left";
    private string leanRightGestureName = "Lean_Right";

    //creating event for my gesture
    private List<GestureDetector> gestureDetectorList = null;

    // Use this for initialization
    void Start()
    {
        turnScript = Player.GetComponent<Turning>();

        kinectSensor = KinectSensor.GetDefault();

        if (kinectSensor != null)
        {
            //몇 명있는지
            bodyCount = kinectSensor.BodyFrameSource.BodyCount;
            //무엇을 하는지
            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            //put the body in array make skeleton data out of it
            bodies = new Body[bodyCount];
            
            //creating new list of gesture
            gestureDetectorList = new List<GestureDetector>();

            for (int bodyIndex = 0; bodyIndex < bodyCount; bodyIndex++)
            {
                gestureDetectorList.Add(new GestureDetector(kinectSensor));
            }

            kinectSensor.Open();
        }
        else
        {
            //kinect sensor not connected
        }
    }

    // Update is called once per frame
    void Update()
    {

            bool data = false;
            using (BodyFrame bodyFrame = bodyFrameReader.AcquireLatestFrame())
            {
            //if this body frame detecting something, data turns true
                if (bodyFrame != null)
                {
                    bodyFrame.GetAndRefreshBodyData(bodies);
                    data = true;
                }
            }

            if (data)
            {
                //이 데이터가 어떤 바디인덱스에 해당하는지
                for (int bodyIndex = 0; bodyIndex < bodyCount; bodyIndex++)
                {
                    var body = bodies[bodyIndex];
                    if (body != null)
                    {
                        //assign trackingId
                        var trackingId = body.TrackingId;
                        if (trackingId != gestureDetectorList[bodyIndex].TrackingId)
                        {
                            gestureDetectorList[bodyIndex].TrackingId = trackingId;
                            gestureDetectorList[bodyIndex].IsPaused = (trackingId == 0);
                            gestureDetectorList[bodyIndex].OnGestureDetected += CreateOnGestureHandler(bodyIndex);
                    }
                    }
                }
            }

    }

    private EventHandler<GestureEventArgs> CreateOnGestureHandler(int bodyIndex)
    {
        return (object sender, GestureEventArgs e) => OnGestureDetected(sender, e, bodyIndex);
    }

    private void OnGestureDetected(object sender, GestureEventArgs e, int bodyIndex)
    {
        var isDetected = e.IsBodyTrackingIdValid && e.IsGestureDetected;

        if(e.GestureID == leanLeftGestureName)
        {
            if (e.DetectionConfidence > 0.65f)
            {
                turnScript.turnLeft = true;
            }
            else
            {
                turnScript.turnLeft = false;
            }
        }

        if (e.GestureID == leanRightGestureName)
        {
            if (e.DetectionConfidence > 0.65f)
            {
                turnScript.turnRight = true;
            }
            else
            {
                turnScript.turnRight = false;
            }
        }

    }


    void OnApplicationQuit()
    {

        if (bodyFrameReader != null)
        {
            bodyFrameReader.Dispose();
            bodyFrameReader = null;
        }

        if (kinectSensor != null)
        {
            if (kinectSensor.IsOpen)
            {
                kinectSensor.Close();
            }

            kinectSensor = null;
        }
    }

}
