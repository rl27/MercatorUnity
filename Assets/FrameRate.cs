    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
     
    // https://forum.unity.com/threads/how-do-i-find-out-how-many-fps-my-project-is-running-at.1111420/
    public class FrameRate : MonoBehaviour {
     
        public int MaxFrames = 60; // Max number of frames to average over
     
        private List<float> frameTimes;
        private float totalTime;
        private static float curFPS;
     
        // Use this for initialization
        void Start () {
            frameTimes = new List<float>();
            totalTime = 0;
            curFPS = 120f; // Affects movespeed at start
            frameTimes.Clear();
        }
     
        // Update is called once per frame
        void Update () {
            float t = Time.unscaledDeltaTime;
            frameTimes.Add(t);
            totalTime += t;
            
            if (frameTimes.Count > MaxFrames)
            {
                totalTime -= frameTimes[0];
                frameTimes.RemoveAt(0);
                curFPS = MaxFrames / totalTime;
            }
        }
     
        public static float GetCurrentFPS()
        {
            return curFPS;
        }
    }
