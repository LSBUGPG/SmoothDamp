using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Graph : MonoBehaviour
{
    public enum SmoothingFunction
    {
        UnitySmoothDamp,
        GraphicsGemsSmoothDamp,
        DogsFix,
        OvershootFix,
    }

    public enum Positioning
    {
        Relative,
        Absolute,
    }

    public LineRenderer distance;
    public LineRenderer velocity;
    public LineRenderer input;
    public LineRenderer position;
    public LineRenderer target;
    public LineRenderer axis;
    public LineRenderer overshoot;

    public SmoothingFunction smoothing;
    public Positioning positioning;
    [Range(0.001f, 1)] public float width = 0.05f;
    [Range(0.001f, 5)] public float smoothTime = 1f;
    [Range(1f, 20f)] public float speed = 1f;
    [Range(0.01f, 0.05f)] public float deltaTime = 0.03333f;
    [Range(1, 10)] public float time = 1f;
    [Range(0f, 2f)] public float positive = 1f;
    [Range(0f, 2f)] public float negative = 1f;
    [Range(0f, 2f)] public float neutral = 1f;
    [Range(1f, 10f)] public float inputChangeVelocity = 3f;
    [Range(0f, 1f)] public float inspect = 0f;
    [Range(0f, 1f)] public float cf;

    [HideInInspector] public float inspectDistance;
    [HideInInspector] public float inspectVelocity;
    [HideInInspector] public float inspectInput;
    [HideInInspector] public float inspectPosition;
    [HideInInspector] public float inspectTarget;
    [HideInInspector] public int inspectStep;

    IEnumerable<float> GenerateInput()
    {
        float input = 0f;
        while (true)
        {
            float time;
            for (time = 0; time < positive; time += deltaTime)
            {
                input = Mathf.MoveTowards(input, 1f, inputChangeVelocity * deltaTime);
                yield return input;
            }
            for (time = 0; time < neutral; time += deltaTime)
            {
                input = Mathf.MoveTowards(input, 0f, inputChangeVelocity * deltaTime);
                yield return input;
            }
            for (time = 0; time < negative; time += deltaTime)
            {
                input = Mathf.MoveTowards(input, -1f, inputChangeVelocity * deltaTime);
                yield return input;
            }
            for (time = 0; time < neutral; time += deltaTime)
            {
                input = Mathf.MoveTowards(input, 0f, inputChangeVelocity * deltaTime);
                yield return input;
            }
        }
    }

    void ConfigureLineRenderer(LineRenderer lineRenderer, int steps, Color color)
    {
        lineRenderer.positionCount = steps;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    void Update()
    {
        int steps = Mathf.RoundToInt(time / deltaTime);
        float x = 20f / steps;
        inspectStep = Mathf.RoundToInt(inspect * steps);

        ConfigureLineRenderer(distance, steps, Color.red);
        ConfigureLineRenderer(velocity, steps, Color.green);
        ConfigureLineRenderer(input, steps, Color.blue);
        ConfigureLineRenderer(position, steps, Color.yellow);
        ConfigureLineRenderer(target, steps, Color.magenta);
        ConfigureLineRenderer(overshoot, steps, Color.cyan);
        axis.SetPosition(0, new Vector3(x * inspectStep, 10f, 0));
        axis.SetPosition(1, new Vector3(x * inspectStep, -10f, 0));

        float objectPosition = 0f;
        float objectVelocity = 0f;
        float targetPosition = 0f;
        var inputGenerator = GenerateInput().GetEnumerator();
        for (int i = 0; i < steps; ++i)
        {
            float inputValue = inputGenerator.Current;
            inputGenerator.MoveNext();

            float previousTarget = targetPosition;
            switch (positioning)
            {
                case Positioning.Relative:
                    targetPosition = objectPosition + inputValue * speed;
                    break;
                case Positioning.Absolute:
                    targetPosition += inputValue * speed * deltaTime;
                    break;
            }

            if (i == inspectStep)
            {
                inspectInput = inputValue;
                inspectTarget = targetPosition;
            }

            float overshotAmount = 0;
            switch (smoothing)
            {
                case SmoothingFunction.UnitySmoothDamp:
                    objectPosition = Mathf.SmoothDamp(objectPosition, targetPosition, ref objectVelocity, smoothTime, Mathf.Infinity, deltaTime);
                    break;
                case SmoothingFunction.GraphicsGemsSmoothDamp:
                    objectPosition = SmoothCD.Original(objectPosition, targetPosition, ref objectVelocity, smoothTime, Mathf.Infinity, deltaTime);
                    break;
                case SmoothingFunction.DogsFix:
                    objectPosition = SmoothCD.DogsFix(objectPosition, targetPosition, ref objectVelocity, smoothTime, Mathf.Infinity, deltaTime);
                    break;
                case SmoothingFunction.OvershootFix:
                    // if (objectVelocity > 0f && previousTarget > objectPosition && targetPosition < objectPosition)
                    // {
                    //     objectVelocity = (targetPosition - objectPosition) / deltaTime;
                    //     objectPosition = targetPosition;
                    // }
                    // else if (objectVelocity < 0f && previousTarget < objectPosition && targetPosition > objectPosition)
                    // {
                    //     objectVelocity = (targetPosition - objectPosition) / deltaTime;
                    //     objectPosition = targetPosition;
                    // }
                    // else
                    {
                        // Prevent overshooting
                        float targetVelocity = Mathf.Clamp((targetPosition - previousTarget) / deltaTime, -speed, speed);
                        if (positioning == Positioning.Relative)
                        {
                            targetVelocity = inputValue * speed;
                        }
                        float projectedTarget = targetPosition + targetVelocity * smoothTime;
                        float projectedPosition = objectPosition + objectVelocity * smoothTime;
                        float direction = targetPosition - objectPosition;
                        float overshoot = projectedTarget - projectedPosition;
                        if ((direction > 0f && overshoot < 0f) || (direction < 0f && overshoot > 0f))
                        {
                            overshotAmount = overshoot;
                            objectVelocity += Mathf.Lerp(0, overshoot / smoothTime, cf);
                            //objectVelocity *= cf;//Mathf.Lerp((projectedTarget - objectPosition) / smoothTime, objectVelocity, cf);
                        }
                        objectPosition = SmoothCD.Original(objectPosition, targetPosition, ref objectVelocity, smoothTime, Mathf.Infinity, deltaTime);
                    }
                    break;
            }

            distance.SetPosition(i, new Vector3(i * x, targetPosition - objectPosition, 0));
            velocity.SetPosition(i, new Vector3(i * x, objectVelocity, 0));
            input.SetPosition(i, new Vector3(i * x, inputValue, 0));
            position.SetPosition(i, new Vector3(i * x, objectPosition, 0));
            target.SetPosition(i, new Vector3(i * x, targetPosition, 0));
            overshoot.SetPosition(i, new Vector3(i * x, overshotAmount, 0));

            if (i == inspectStep)
            {
                inspectDistance = targetPosition - objectPosition;
                inspectVelocity = objectVelocity;
                inspectPosition = objectPosition;
            }
        }
    }
}
