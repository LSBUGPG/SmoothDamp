using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Graph : MonoBehaviour
{
    public enum SmoothingFunction
    {
        GraphicsGemsSmoothDamp,
        UnitySmoothDamp,
        SmoothDampZeroCheck,
        SmoothDampMovingTarget,
    }

    public enum Targeting
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

    public SmoothingFunction smoothing;
    public Targeting targeting;
    [Range(0.001f, 1)] public float width = 0.05f;
    [Range(0.001f, 5)] public float smoothTime = 1f;
    [Range(0.5f, 100f)] public float speed = 1f;
    [Range(0.01f, 0.05f)] public float deltaTime = 0.03333f;
    [Range(1, 10)] public float time = 1f;
    [Range(0f, 2f)] public float positive = 1f;
    [Range(0f, 2f)] public float negative = 1f;
    [Range(0f, 5f)] public float neutral = 1f;
    [Range(1f, 100f)] public float inputChangeVelocity = 3f;
    [Range(0f, 1f)] public float inspect = 0f;
    [Range(1f, 20f)] public float maxSpeed = 3f;

    [HideInInspector] public float inspectDistance;
    [HideInInspector] public float inspectVelocity;
    [HideInInspector] public float inspectInput;
    [HideInInspector] public float inspectPosition;
    [HideInInspector] public float inspectTarget;
    [HideInInspector] public int inspectStep;
    [HideInInspector] public float inspectTime;

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
            switch (targeting)
            {
                case Targeting.Relative:
                    targetPosition = objectPosition + inputValue * speed;
                    break;
                case Targeting.Absolute:
                    targetPosition += inputValue * speed * deltaTime;
                    break;
            }

            if (i == inspectStep)
            {
                inspectInput = inputValue;
                inspectTarget = targetPosition;
                inspectTime = i * deltaTime;
                inspectDistance = targetPosition - objectPosition;
                inspectVelocity = objectVelocity;
                inspectPosition = objectPosition;
            }

            distance.SetPosition(i, new Vector3(i * x, targetPosition - objectPosition, 0));
            velocity.SetPosition(i, new Vector3(i * x, objectVelocity, 0));
            input.SetPosition(i, new Vector3(i * x, inputValue, 0));
            position.SetPosition(i, new Vector3(i * x, objectPosition, 0));
            target.SetPosition(i, new Vector3(i * x, previousTarget, 0));

            switch (smoothing)
            {
                case SmoothingFunction.GraphicsGemsSmoothDamp:
                    objectPosition = SmoothCD.Original(objectPosition, targetPosition, ref objectVelocity, smoothTime, maxSpeed, deltaTime);
                    break;
                case SmoothingFunction.UnitySmoothDamp:
                    objectPosition = Mathf.SmoothDamp(objectPosition, targetPosition, ref objectVelocity, smoothTime, maxSpeed, deltaTime);
                    break;
                case SmoothingFunction.SmoothDampZeroCheck:
                    objectPosition = SmoothCD.SmoothDampZeroCheck(objectPosition, targetPosition, ref objectVelocity, smoothTime, maxSpeed, deltaTime);
                    break;
                case SmoothingFunction.SmoothDampMovingTarget:
                    objectPosition = SmoothCD.SmoothDampMovingTarget(objectPosition, targetPosition, ref objectVelocity, previousTarget, smoothTime, maxSpeed, deltaTime);
                    break;
            }

        }
    }
}
