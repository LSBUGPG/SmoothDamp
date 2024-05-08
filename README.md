# SmoothDamp

This project contains my investigations into the [Unity SmoothDamp bug](https://issuetracker.unity3d.com/issues/smoothdamp-behaves-differently-between-positive-and-negative-velocity). We noticed the issue using SmoothDampAngle where the behaviour seemed different when rotating clockwise vs anti-clockwise. Researching this problem turned up [this thread](https://forum.unity.com/threads/smoothdamp-problem.99474/) in the Unity forums.

The origin of the Unity SmoothDamp function is from the book [Game Programming Gems 4](https://archive.org/embed/gameprogrammingg0000unse) chapter 1.10 pages 95-101. This contains the source code in C++ and a description of how to extend the function to provide `maxSpeed`.

Here is the original code converted to C#

```.cs
    public static float Original(float from, float to, ref float vel, float smoothTime, float deltaTime)
    {
        float omega = 2f / smoothTime;
        float x = omega * deltaTime;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
        float change = from - to;
        float temp = (vel + omega * change) * deltaTime;
        vel = (vel - omega * temp) * exp; // Equation 5
        return to + (change + temp) * exp; // Equation 4
    }
```
