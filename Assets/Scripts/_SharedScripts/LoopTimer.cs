using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopTimer
{
    public Action LapAction { get; set; }
    public Action FinishAction { get; set; }
    public float? LapTime { get; set; }
    public float? FinishTime { get; set; }

    private bool dead = true;
    private float elapsedTime = 0;
    private float currentLapTime = 0;
    private int currentLap = 1;

    public LoopTimer()
    {
        LapAction = null;
        FinishAction = null;
        LapTime = null;
        FinishTime = null;
    }

    public LoopTimer(float lap)
    {
        LapAction = null;
        FinishAction = null;
        LapTime = lap;
        FinishTime = null;
    }

    public LoopTimer(Action act, float finish)
    {
        LapAction = null;
        FinishAction = act;
        LapTime = null;
        FinishTime = finish;
    }

    public LoopTimer(Action act, float finish, float lap)
    {
        LapAction = act;
        FinishAction = act;
        LapTime = lap;
        FinishTime = finish;
    }

    public LoopTimer(Action finAct, Action lapAct, float finish, float lap)
    {
        LapAction = lapAct;
        FinishAction = finAct;
        LapTime = lap;
        FinishTime = finish;
    }

    public void Start()
    {
        dead = false;
    }

    public void Reset()
    {
        elapsedTime = 0;
        currentLapTime = 0;
        currentLap = 1;
        Start();
    }

    public void Pause()
    {
        dead = true;
    }

    public void Update(float time)
    {
        if (dead)
            return;

        elapsedTime += time;
        currentLapTime += time;

        if (FinishTime != null && elapsedTime >= FinishTime)
        {
            elapsedTime = (float)FinishTime;
            if (FinishAction != null)
                FinishAction();

            dead = true;
            return;
        }

        if (LapTime != null && currentLapTime >= LapTime)
        {
            currentLap += 1;
            currentLapTime = currentLapTime - (float)LapTime;
            if (LapAction != null)
                LapAction();
        }
    }

    public float TimePassed()
    {
        return elapsedTime;
    }

    public int GetCurrentLap()
    {
        return currentLap;
    }

    public bool IsDead()
    {
        return dead;
    }
}
