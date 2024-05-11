using UnityEditor;
using UnityEngine;

public static class SmoothCD
{
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

    public static float Original(float from, float to, ref float vel, float smoothTime, float maxSpeed, float deltaTime)
    {
        float omega = 2f / smoothTime;
        float x = omega * deltaTime;
        float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
        float change = to - from;
        // Clamp maximum speed
        float maxChange = maxSpeed * smoothTime;
        change = Mathf.Clamp(change, -maxChange, maxChange);
        float temp = (vel - omega * change) * deltaTime;
        vel = (vel - omega * temp) * exp;
        return from + change + (temp - change) * exp;
    }

    public static float SmoothDampZeroCheck(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
    {
        // Based on Game Programming Gems 4 Chapter 1.10
        smoothTime = Mathf.Max(0.0001F, smoothTime);
        float omega = 2F / smoothTime;
        
        float x = omega * deltaTime;
        float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
        float change = current - target;
        float originalTo = target;

        // Clamp maximum speed
        float maxChange = maxSpeed * smoothTime;
        change = Mathf.Clamp(change, -maxChange, maxChange);
        target = current - change;
        
        float temp = (currentVelocity + omega * change) * deltaTime;
        currentVelocity = (currentVelocity - omega * temp) * exp;
        float output = target + (change + temp) * exp;
    
        // Prevent overshooting
        if (originalTo == current || originalTo > current == output > originalTo)
        {
            output = originalTo;
            currentVelocity = (output - originalTo) / deltaTime;
        }
        
        return output;
    }

    public static float SmoothDampTest(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
    {
        // Based on Game Programming Gems 4 Chapter 1.10
        smoothTime = Mathf.Max(0.0001F, smoothTime);
        float omega = 2F / smoothTime;
        
        float x = omega * deltaTime;
        float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
        float change = current - target;
        float originalTo = target;

        // float maxVelocity = -change * (1 + x) / deltaTime;
        // if (currentVelocity > 0f && currentVelocity > maxVelocity || currentVelocity < 0f && currentVelocity < maxVelocity)
        // {
        //     currentVelocity = maxVelocity;
        //     return target;
        // }

        // Clamp maximum speed
        float maxChange = maxSpeed * smoothTime;
        change = Mathf.Clamp(change, -maxChange, maxChange);
        target = current - change;
        
        float temp = (currentVelocity + omega * change) * deltaTime;
        currentVelocity = (currentVelocity - omega * temp) * exp;
        float output = target + (change + temp) * exp;
    
        // Prevent overshooting
        if (originalTo == current || originalTo > current == output > originalTo)
        {
            output = originalTo;
            currentVelocity = (output - originalTo) / deltaTime;
        }
        
        return output;
    }

    public static float PreventOvershoot(float current, float target, ref float velocity, float targetVelocity, float smoothTime, float maxSpeed, float deltaTime, float overshootReduction)
    {
        // predict the overshoot
        float projectedTarget = target + targetVelocity * smoothTime;
        float projectedPosition = current + velocity * smoothTime;
        float overshoot = projectedPosition - projectedTarget;

        // adjust the target to prevent overshooting
        target -= overshoot * overshootReduction;

        return Original(current, target, ref velocity, smoothTime, maxSpeed, deltaTime);
    }
}
