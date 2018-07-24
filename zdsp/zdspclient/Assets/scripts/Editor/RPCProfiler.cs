using System;
using UnityEngine;
using UnityEditor;

namespace Assets.scripts.Editor
{
    public class RPCProfilerWindow : EditorWindow
    {
        public static RPCProfilerWindow window;
        public AnimationCurve byteReceivedCurve, byteSentCurve;
        public float timeElapsed;

        public int totalBytesReceived;
        public int biggestBytesReceivedInFrame;
        public int totalBytesSent;
        public int biggestBytesSentInFrame;

        public int[] frameBytesReceived;
        public int[] frameBytesSent;
        public int nextFrameIndex;
        private int downloadBPS; //bits per second
        private int uploadBPS;

        [MenuItem("Window/Zealot Tools/RPC Profiler")]
        static void ShowRPCProfilerWindow()
        {
            window = (RPCProfilerWindow) EditorWindow.GetWindow(typeof(RPCProfilerWindow));
            //window.position = new Rect(10, 50, 1100, 500);
            window.titleContent = new GUIContent("RPC Profiler");
            //window.autoRepaintOnSceneChange = true;
            
            window.ShowUtility();
            
            window.byteReceivedCurve = AnimationCurve.Linear(0, 0, 0, 0);
            window.timeElapsed = 0;
            window.totalBytesReceived = 0;
            window.biggestBytesReceivedInFrame = 0;

            window.byteSentCurve = AnimationCurve.Linear(0, 0, 0, 0);
            window.totalBytesSent = 0;
            window.biggestBytesSentInFrame = 0;

            window.frameBytesReceived = new int[10]; //10 frames = 1sec
            window.frameBytesSent = new int[10]; //10 frames = 1sec
            window.nextFrameIndex = 0;
        }

        void Update()
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed > 0.1f) //Record keyframe every 100 msec
            {
                timeElapsed = 0;
                int bytesReceived = RPCFactory.GetTotalBytesReceived();
                
                Keyframe keyframe = new Keyframe(Time.time, bytesReceived);
                if (byteReceivedCurve.length > 299) //delete after 300* 100 msec = 30secs
                    byteReceivedCurve.MoveKey(0, keyframe);
                else
                    byteReceivedCurve.AddKey(keyframe);

                totalBytesReceived += bytesReceived;

                if (bytesReceived > biggestBytesReceivedInFrame)
                    biggestBytesReceivedInFrame = bytesReceived;

                frameBytesReceived[nextFrameIndex] = bytesReceived;


                //////////////////////////////////////////////////
                int bytesSent = RPCFactory.GetTotalBytesSent();
                keyframe = new Keyframe(Time.time, bytesSent);
                if (byteSentCurve.length > 299) //delete after 300* 100 msec = 30secs
                    byteSentCurve.MoveKey(0, keyframe);
                else
                    byteSentCurve.AddKey(keyframe);

                totalBytesSent += bytesSent;

                if (bytesSent > biggestBytesSentInFrame)
                    biggestBytesSentInFrame = bytesSent;

                frameBytesSent[nextFrameIndex] = bytesSent;

                RPCFactory.ResetProfilerForNextFrame();
                Repaint();

                nextFrameIndex++;
                if (nextFrameIndex >= 10)
                    nextFrameIndex = 0;

                int totalBytesReceivedIn1Sec = 0;
                int totalBytesSentIn1Sec = 0;
                for (int i=0;i<10;i++)
                {
                    totalBytesReceivedIn1Sec += frameBytesReceived[i];
                    totalBytesSentIn1Sec += frameBytesSent[i];
                }
                downloadBPS = totalBytesReceivedIn1Sec * 8;
                uploadBPS = totalBytesSentIn1Sec * 8;
            }
            
        }

        void OnGUI()
        {
            if (window == null)
            {
                ShowRPCProfilerWindow();
            }

            float displayDuration = 30.0f;
            AnimationCurve temp = new AnimationCurve(byteReceivedCurve.keys);
            Keyframe lastKey = temp.keys[temp.length - 1];
            float endTime = lastKey.time;

            GUI.Label(new Rect(3, 3, position.width - 6, 100), string.Format("Time: {0} sec", Math.Round(endTime, 2)));                        
            GUI.Label(new Rect(3, 20, position.width - 6, 100), string.Format("Total Bytes Received so far: {0} bytes", totalBytesReceived));
            GUI.Label(new Rect(3, 37, position.width - 6, 100), string.Format("Max Bytes Received in 1 frame: {0} bytes", biggestBytesReceivedInFrame));
            float downloadkbps = downloadBPS / 1024.0f;
            GUI.Label(new Rect(3, 54, position.width - 6, 100), string.Format("Download rate: {0} kbps", downloadkbps));

            if (endTime < displayDuration)
                endTime = displayDuration;

            float startTime = lastKey.time - 30.0f;
            int maxY = 5000;
            //if (biggestBytesReceivedInFrame > maxY)
            //    maxY = biggestBytesReceivedInFrame;
            

            EditorGUI.CurveField(new Rect(3, 71, position.width - 6, 100), "Bytes Received(All RPCs)", temp, Color.cyan, new Rect(startTime, 0, displayDuration, maxY));
            //Range: Rect[x-axis min, y-axix min, x-axis max, y-axis max]


            //----------------------------------------------------------------------------------

            GUI.Label(new Rect(3, 167, position.width - 6, 100), string.Format("Total Bytes Sent so far: {0} bytes", totalBytesSent));
            GUI.Label(new Rect(3, 184, position.width - 6, 100), string.Format("Max Bytes Sent in 1 frame: {0} bytes", biggestBytesSentInFrame));
            float uploadkbps = uploadBPS / 1024.0f;
            GUI.Label(new Rect(3, 201, position.width - 6, 100), string.Format("Upload rate: {0} kbps", uploadkbps));

            if (biggestBytesSentInFrame > maxY)
                maxY = biggestBytesSentInFrame;

            temp = new AnimationCurve(byteSentCurve.keys);
            EditorGUI.CurveField(new Rect(3, 218, position.width - 6, 100), "Bytes Sent(All RPCs)", temp, Color.cyan, new Rect(startTime, 0, displayDuration, maxY));
        }
    }
}
