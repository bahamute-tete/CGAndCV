
using UnityEngine;
using UnityEngine.Events;

public class AutomaticSlider : MonoBehaviour
{
    [SerializeField, Min(0f)] float duration = 1f;
    float value;

    [SerializeField] bool autoReverse = false, smoothStep = false;
   
    public bool Reversed { get; set; }
    public bool AutoReversed
    {
        get => autoReverse;
        set => autoReverse = value;
    }


    float SmoothValue => 3f * value * value - 2f * value * value * value;

    [System.Serializable]
    public class OnValueChangedEvent : UnityEvent<float> { }

    [SerializeField] OnValueChangedEvent onValueChanged = default;

    void FixedUpdate()
    {
        float delta = Time.deltaTime / duration;
        if (Reversed)
        {
            value -= delta;
            if (value <= 0f)
            {
                if (autoReverse)
                {
                    value = Mathf.Min(1f, -value);
                    Reversed = false;
                }
                else
                {
                    value = 0f;
                    enabled = false;
                }
            }
        }
        else
        {
            value += delta;
            if (value >= 1f)
            {
                if (autoReverse)
                {
                    value = Mathf.Max(0f, 2f - value);
                    Reversed = true;
                }
                else
                {
                    value = 1f;
                    enabled = false;
                }
            }
        }
        onValueChanged.Invoke(smoothStep?SmoothValue:value);
    }
}
