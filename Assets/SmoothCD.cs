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
        float output = Original(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    
        // Prevent overshooting
        if (target == current || target > current == output > target)
        {
            output = target;
            currentVelocity = (target - output) / deltaTime;
        }
        
        return output;
    }

    public static float SmoothDampMovingTarget(float current, float target, ref float currentVelocity, float previousTarget, float smoothTime, float maxSpeed, float deltaTime)
    {
        float output;
        if (target == current || (previousTarget < current && current < target) || (previousTarget > current && current > target))
        {
            // currently on target or target is passing through
            output = current;
            currentVelocity = 0f;
        }
        else
        {
            
            // apply original smoothing
            output = Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
            if ((target > current && output > target) || (target < current && output < target))
            {
                // we have overshot the target
                output = target;
                currentVelocity = 0f;
            }
        }
        return output;
    }
}
