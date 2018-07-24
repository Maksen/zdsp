using System;
using UnityEngine;
using Zealot.Common;
using System.Collections.Generic;
using System.Linq;

public class TickerTapeSystem : MonoSingleton<TickerTapeSystem>
{
    private List<GMMessageData> messageList;
    private int day;
    private Queue<GMMessageData> activeQueue;
    private Queue<GMMessageData> waitQueue;
    private readonly int[] priorityList = new int[] { 3, 2, 1 };
    private class MessageData
    {
        public float lastQueryTime = 0;
        public bool isWaiting = false;
    }

    private Dictionary<int, MessageData> messageRecordMap = new Dictionary<int, MessageData>();

    public void OnLogin()
    {
        activeQueue = new Queue<GMMessageData>();
        waitQueue = new Queue<GMMessageData>();
        messageList = new List<GMMessageData>();
        day = DateTime.Today.Day;
    }

    public void RefreshGMMessage(List<GMMessageData> messageList)
    {
        this.messageList = messageList;
        messageList.ForEach(e => e.CheckIsActive());
        ProcessSchedule();
    }

    private void CheckChangeDay()
    {
        if (DateTime.Today.Day != day)
        {
            messageList.ForEach(e => e.CheckIsActive());
            day = DateTime.Today.Day;
        }
    }
    
    public bool QueryMessage(out string message)
    {
        if (activeQueue.Count == 0)
        {
            message = string.Empty;
            return false;
        }

        var first = activeQueue.First();
        if (first.IsExpired() == true)
        {
            activeQueue.Dequeue();
            return QueryMessage(out message);
        }

        if (first.isActive == false || first.IsStandby() == true)
        {
            waitQueue.Enqueue(activeQueue.Dequeue());
            return QueryMessage(out message);
        }

        if (first.type == GMMessageType.Timing)
        {
            var today = DateTime.Today;
            var start = today + new TimeSpan(first.hour, first.min, 0);
            var end = start + new TimeSpan(0, first.duration, 0);
            if (DateTime.Now < start || DateTime.Now >= end)
            {
                waitQueue.Enqueue(activeQueue.Dequeue());
                return QueryMessage(out message);
            }
        }

        if (first.type == GMMessageType.Interval)
        {
            if (messageRecordMap.ContainsKey(first.id))
            {
                var data = messageRecordMap[first.id];
                if (data.isWaiting == true)
                {
                    if (Time.time - data.lastQueryTime > first.interval * 60)
                    {
                        data.lastQueryTime = Time.time;
                        data.isWaiting = false;
                        message = first.message;
                        waitQueue.Enqueue(activeQueue.Dequeue());
                        return true;
                    }
                    else
                    {
                        waitQueue.Enqueue(activeQueue.Dequeue());
                        return QueryMessage(out message);
                    }
                }
                else
                {
                    if (Time.time - data.lastQueryTime > first.duration * 60)
                    {
                        data.lastQueryTime = Time.time;
                        data.isWaiting = true;
                        waitQueue.Enqueue(activeQueue.Dequeue());
                        return QueryMessage(out message);
                    }
                    else
                    {
                        message = first.message;
                        waitQueue.Enqueue(activeQueue.Dequeue());
                        return true;
                    }
                }
            }
            else
            {
                var data = new MessageData();
                messageRecordMap[first.id] = data;
                data.lastQueryTime = Time.time;
                data.isWaiting = false;
                message = first.message;
                waitQueue.Enqueue(activeQueue.Dequeue());
                return true;
            }
        }

        message = first.message;
        waitQueue.Enqueue(activeQueue.Dequeue());
        return true;
    }

    public void Reset()
    {
        if (activeQueue.Count == 0 && waitQueue.Count > 0)
        {
            var tmp = activeQueue;
            activeQueue = waitQueue;
            waitQueue = tmp;
        }
    }

    private void ProcessSchedule()
    {
        messageList.Sort((a, b) =>
        {
            if (priorityList[(int)a.type] < priorityList[(int)b.type])
                return 1;

            if (priorityList[(int)a.type] > priorityList[(int)b.type])
                return -1;

            if (a.interval > b.interval)
                return 1;

            if (a.interval < b.interval)
                return -1;

            if (a.duration > b.duration)
                return 1;

            if (a.duration < b.duration)
                return -1;

            if (a.updatetime < b.updatetime)
                return 1;

            if (a.updatetime > b.updatetime)
                return -1;

            if (a.message.Length > b.message.Length)
                return 1;

            if (a.message.Length < b.message.Length)
                return -1;

            return 0;
        });

        activeQueue = new Queue<GMMessageData>(messageList);
        waitQueue.Clear();
    }

    void Update()
    {
        CheckChangeDay();
    }
}
