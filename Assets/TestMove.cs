using UnityEngine;

public class TestMove : MonoBehaviour
{
    public enum SmoothingFunction
    {
        Original,
        Unity,
        DogsFix,
        OvershootFix,
    }

    public enum Positioning
    {
        Relative,
        Absolute,
    }

    public SmoothingFunction smoothing;
    public Positioning positioning;
    public float smoothTime = 1f;
    public float speed = 5f;
    float velocity = 0;
    float target = 0;

    void Update()
    {
        Vector3 position = transform.position;
        float input = Input.GetAxis("Vertical");
        switch (positioning)
        {
            case Positioning.Relative:
                target = position.y + input * speed;
                break;
            case Positioning.Absolute:
                target += input * speed * Time.deltaTime;
                break;
        }

        switch (smoothing)
        {
            case SmoothingFunction.Original:
                position.y = SmoothCD.Original(position.y, target, ref velocity, smoothTime, Time.deltaTime);
                break;
            case SmoothingFunction.Unity:
                position.y = Mathf.SmoothDamp(position.y, target, ref velocity, smoothTime, Mathf.Infinity, Time.deltaTime);
                break;
            case SmoothingFunction.DogsFix:
                position.y = SmoothCD.DogsFix(position.y, target, ref velocity, smoothTime, Mathf.Infinity, Time.deltaTime);
                break;
            case SmoothingFunction.OvershootFix:
                position.y = SmoothCD.Original(position.y, target, ref velocity, smoothTime, Mathf.Infinity, Time.deltaTime);
                break;
        }
        transform.position = position;
    }
}
