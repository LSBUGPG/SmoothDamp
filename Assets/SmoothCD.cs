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