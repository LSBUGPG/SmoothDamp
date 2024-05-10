# SmoothDamp

This project contains my investigations into the [Unity SmoothDamp bug](https://issuetracker.unity3d.com/issues/smoothdamp-behaves-differently-between-positive-and-negative-velocity). We noticed the issue using SmoothDampAngle where the behaviour seemed different when rotating clockwise vs anti-clockwise. Researching this problem turned up [this thread](https://forum.unity.com/threads/smoothdamp-problem.99474/) in the Unity forums.

The origin of the Unity SmoothDamp function is from the book [Game Programming Gems 4](https://archive.org/embed/gameprogrammingg0000unse) chapter 1.10 pages 95-101. This contains the source code in C++ and a description of how to extend the function to provide `maxSpeed`.

Here is the original code converted to C#

```csharp
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

Unfortunately, the extension for `maxSpeed` is not fully described, and simply adding the code given in the book doesn't work. Instead I had to re-arrange the expressions to make it work.

```csharp
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
```

A Unity developer posted the source code that Unity uses as a comment in the above bug report:

```csharp
public static float SmoothDampUnity(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
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
    if (originalTo - current > 0.0F == output > originalTo)
    {
        output = originalTo;
        currentVelocity = (output - originalTo) / deltaTime;
    }
    
    return output;
}
```

## The bug

The inconsistent behaviour comes from the code Unity added to prevent overshooting:

```csharp
    // Prevent overshooting
    if (originalTo - current > 0.0F == output > originalTo)
    {
        output = originalTo;
        currentVelocity = (output - originalTo) / deltaTime;
    }
```

One issue with this code is in the conditional. It is a rare example of an `XNOR` (the opposite of an exclusive or) which will be `true` if both sides are `true` or if both are `false`. The first condition `originalTo - current > 0.0F` asks if we are moving in a positive direction, the second asks if the output would take us beyond the target. The opposite cases ought to be if we are moving in a negative direction and our output would be before the target, but it includes the case that we are not moving and the output matches the target.

## The use cases

There are two commonly used methods to set the target position for the `SmoothDamp` function. I call these, the `relative` and `absolute` methods.

### Absolute target

Using this method we maintain a target position and modify it with time adjusted input:

```csharp
    target += input * speed * Time.deltaTime;
```

This method produces smooth movement and the smoothed object can take a while to reach the target. Note, despite the documentation, it takes much longer than the `smoothTime` to reach the target. The original Gems article notes that a good definition for `smoothTime` is "the expected time to reach the target _when at maximum velocity_." However, since it takes some time to reach maximum velocity and it falls off as you approach the target it takes longer than `smoothTime` alone.

### Relative target

Using this method we position the target relative to the current position based on the input:

```csharp
    target = current + input * speed;
```

This method responds to changes much faster than the absolute method and by definition it reaches target as soon as the input drops to zero. Note that the original Gems version of the function allows the velocity value to come to a smooth halt. Whereas the Unity version attempts to zero out the velocity once the input drops to zero. This (I believe) is the overshoot that they are attempting to prevent with the additional code. As noted earlier, this happens inconsistently in the Unity code.

## Testing the output


