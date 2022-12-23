using UnityEngine;

public class Accelerometer : MonoBehaviour
{
    float speed = 10.0f;

    float accelerometerUpdateInterval = 1.0f / 60.0f;

    //The greater the value of LowPassKernelWidthInSeconds,
    //the slower the filtered value will converge towards the current input sample (and vice versa).
    [SerializeField]float lowPassKernelWidthInSeconds = 1.0f;

    private float lowPassFilterFactor;
    private Vector3 lowPassValue = Vector3.zero;

    void Start()
    {
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
        lowPassValue = Input.acceleration;

        Input.gyro.enabled = true;
    }

    void Update()
    {
        Vector3 dir = Vector3.zero;

        Vector3 newAccerlation = Input.acceleration;
        //Vector3 newAccerlation = Input.gyro.gravity;
        //Debug.Log($"x={newAccerlation.x}\n" +
        //          $"y={newAccerlation.y}\n" +
        //          $"z={newAccerlation.z}\n");

        Vector3 angle = Input.gyro.attitude.eulerAngles;
        Quaternion q = Input.gyro.attitude;

        Debug.Log($"x={angle.x}" +$"  y={angle.y}" + $"  z={angle.z}");

        transform.rotation = Quaternion.Slerp(transform.rotation, new Quaternion(-q.x, -q.y, q.z, q.w), 0.5f);
        //transform.rotation = Quaternion.Slerp(transform.rotation, new Quaternion(q.x, q.y, q.z, q.w), 0.5f);
        //Debug.Log($"xChnage ={(newAccerlation.x - lowPassValue.x)*100}" +
        //          $"yChange ={(newAccerlation.y - lowPassValue.y)*100}" +
        //          $"zChange ={(newAccerlation.z - lowPassValue.z)*100}");

        lowPassValue = newAccerlation;

        
        //lowPassValue = LowPassFilterAccelerometer(lowPassValue);




        //dir.x = lowPassValue.x;
        //dir.z = lowPassValue.y;

        //Debug.Log($"x ={Input.acceleration.x},y ={Input.acceleration.y}");

        //// clamp acceleration vector to the unit sphere
        //if (dir.sqrMagnitude > 1)
        //    dir.Normalize();

        //// Make it move 10 meters per second instead of 10 meters per frame...
        //dir *= Time.deltaTime;

        //// Move object
        //transform.Translate(dir * speed);
    }


    Vector3 LowPassFilterAccelerometer(Vector3 prevValue)
    {
        Vector3 newValue = Vector3.Lerp(prevValue, Input.acceleration, lowPassFilterFactor);
        return newValue;
    }


    private void OnDrawGizmos()
    {
        Vector3 dir = Input.acceleration.normalized;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(Vector3.zero, dir * 1f);
    }
}