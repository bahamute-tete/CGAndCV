using System.Collections.Generic;
using UnityEngine;

public class SDKGyroController : MonoBehaviour
{
    //陀螺仪是否存在

    class GyroGameObject
    {
        public GameObject go = null;
        public bool m_IsRotate = false;
        public bool m_IsBack = false;
        public Vector3 m_StartVec3 = Vector3.zero;
        public float m_RangeX = 0;
        public float m_RangeY = 0;
        public float m_SpeedX = 0;
        public float m_SpeedY = 0;
        public float m_LerpParam = 0;
        public float lastDiff_X;
        public float lastDiff_Y;
        public void Clear()
        {
            go = null;
        }
        public void ReSet()
        {
            if (go != null)
            {
                go.transform.localPosition = m_StartVec3;
            }
        }
    }
    private List<GyroGameObject> gyroGameObjects = new List<GyroGameObject>();

    private bool gyroBool;
    public bool isGyro = true;
    //陀螺仪
    private Gyroscope gyro = null;

    private float m_StartGyroX;
    private float m_StartGyroY;

    //陀螺仪x轴的取值
    private float m_CurGyroX = 0f;
    private float m_CurGyroY = 0f;

    private float diff_X = 0f;
    private float diff_Y = 0f;

    private float valueGyroX = 0f;
    private float valueGyroY = 0f;
    // 调用，用来添加一个控制器脚本
    public static void AddGyroComponent(GameObject gameObject, float rangeX, float rangeY, float speedX, float speedY, float lerpValue)
    {
        SDKGyroController gm = gameObject.GetComponent<SDKGyroController>();
        if (gm == null)
            gm = gameObject.AddComponent<SDKGyroController>();
        gm.AddGyroGameObject(gameObject, rangeX, rangeY, speedX, speedY, lerpValue);
    }
    //调用，用来添加一个GameObject做移动旋转效果
    //默认为移动 isRotate true 为旋转
    //isBack 旋转方向和陀螺仪方向是否相反
    public void AddGyroGameObject(GameObject gameObject, float rangeX, float rangeY, float speedX, float speedY, float lerpValue, bool isRotate = false, bool isBack = false)
    {
        GyroGameObject gyro = new GyroGameObject();
        gyro.go = gameObject;
        gyro.m_IsRotate = isRotate;
        gyro.m_IsBack = isBack;
        if (isRotate)
            gyro.m_StartVec3 = new Vector3(gameObject.transform.localEulerAngles.x, gameObject.transform.localEulerAngles.y, gameObject.transform.localEulerAngles.z);
        else
        {
            gyro.m_StartVec3 = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, gameObject.transform.localPosition.z);
        }
        gyro.m_RangeX = rangeX;
        gyro.m_RangeY = rangeY;
        gyro.m_SpeedX = speedX;
        gyro.m_SpeedY = speedY;
        gyro.m_LerpParam = lerpValue;
        gyroGameObjects.Add(gyro);
    }
    public void Clear()
    {
        for (int i = 0; i < gyroGameObjects.Count; i++)
        {
            if (gyroGameObjects[i] != null)
            {
                gyroGameObjects[i].Clear();
            }
        }
        gyroGameObjects.Clear();
    }
    public void Clear(GameObject go)
    {
        for (int i = 0; i < gyroGameObjects.Count; i++)
        {
            if (gyroGameObjects[i].go == go)
            {
                gyroGameObjects.Remove(gyroGameObjects[i]);
                break;
            }
        }
    }
    void OnDestroy()
    {
        Clear();
    }
    void Awake()
    {
#if !UNITY_EDITOR
        //判断是否支持陀螺仪
        gyroBool = SystemInfo.supportsGyroscope;
#else
        gyroBool = true;
#endif


#if !UNITY_EDITOR
        //给陀螺仪赋值
        gyro = Input.gyro;
        if (gyro != null)
        {
            gyroBool = true;
            gyro.enabled = true;
        }
#endif
    }

    private void Start()
    {
        if (gyroBool)
        {
#if !UNITY_EDITOR
            m_StartGyroX = gyro.gravity.x;
            m_StartGyroY = gyro.gravity.y;
#else
            valueGyroX = 0;
            valueGyroY = 0;
            m_StartGyroX = valueGyroX;
            m_StartGyroY = valueGyroY;
#endif
        }
    }
    public void ReSet()
    {
        for (int i = 0; i < gyroGameObjects.Count; i++)
        {
            if (gyroGameObjects[i] != null) gyroGameObjects[i].ReSet();
        }
    }
    int m_upate_rate = 30;
    void Update()
    {
        Input.gyro.updateInterval = 1.0f / m_upate_rate;
        //位置随着陀螺仪重力感应的X轴变化而变化
        if (gyroBool && isGyro)
        {
            for (int i = 0; i < gyroGameObjects.Count; i++)
            {
                if (gyroGameObjects[i].m_IsRotate)
                {
                    RotateGameObject(gyroGameObjects[i]);
                }
                else
                {
                    TransformGameObject(gyroGameObjects[i]);
                }
            }
        }
    }

    private float curPosX = 0f;
    private float curPosY = 0f;
    //移动操作
    void TransformGameObject(GyroGameObject gyroGameObject)
    {
        if (gyroGameObject == null || gyroGameObject.go == null)
            return;
#if UNITY_EDITOR
        m_CurGyroX = valueGyroX;
        m_CurGyroY = valueGyroY;
#else
        m_CurGyroX = gyro.gravity.x;
        m_CurGyroY = gyro.gravity.y;
#endif
        if (gyroGameObject.m_IsBack)
        {
            m_CurGyroX = 0 - m_CurGyroX;
            m_CurGyroY = 0 - m_CurGyroY;
        }
        diff_X = m_CurGyroX - m_StartGyroX;
        diff_Y = m_CurGyroY - m_StartGyroY;

        float xx = Mathf.Lerp(0, diff_X, gyroGameObject.m_LerpParam);
        float yy = Mathf.Lerp(0, diff_Y, gyroGameObject.m_LerpParam);

        Vector3 v3 = new Vector3(xx * gyroGameObject.m_SpeedX, yy * gyroGameObject.m_SpeedY, 0);

        Vector3 curVec3 = gyroGameObject.go.transform.localPosition;

        if (v3.x > gyroGameObject.m_RangeX)
        {
            curPosX = gyroGameObject.m_StartVec3.x + gyroGameObject.m_RangeX;
            if (gyroGameObject.lastDiff_X > diff_X)
            {
                m_StartGyroX = m_CurGyroX - gyroGameObject.m_RangeX / gyroGameObject.m_SpeedX;
            }
        }
        else if (v3.x < -gyroGameObject.m_RangeX)
        {
            curPosX = gyroGameObject.m_StartVec3.x - gyroGameObject.m_RangeX;
            if (gyroGameObject.lastDiff_X < diff_X)
            {
                m_StartGyroX = m_CurGyroX + gyroGameObject.m_RangeX / gyroGameObject.m_SpeedX;
            }
        }
        else
        {
            float fx = Mathf.Lerp(curVec3.x, gyroGameObject.m_StartVec3.x + v3.x, gyroGameObject.m_LerpParam);
            curPosX = fx;
        }

        if (v3.y > gyroGameObject.m_RangeY)
        {
            curPosY = gyroGameObject.m_StartVec3.y + gyroGameObject.m_RangeY;
            if (gyroGameObject.lastDiff_Y > diff_Y)
            {
                m_StartGyroY = m_CurGyroY - gyroGameObject.m_RangeY / gyroGameObject.m_SpeedY;
            }
        }
        else if (v3.y < -gyroGameObject.m_RangeY)
        {
            curPosY = gyroGameObject.m_StartVec3.y - gyroGameObject.m_RangeY;
            if (gyroGameObject.lastDiff_Y < diff_Y)
            {
                m_StartGyroY = m_CurGyroY + gyroGameObject.m_RangeY / gyroGameObject.m_SpeedY;
            }
        }
        else
        {
            float fy = Mathf.Lerp(curVec3.y, gyroGameObject.m_StartVec3.y + v3.y, gyroGameObject.m_LerpParam);
            curPosY = fy;
        }

        gyroGameObject.go.transform.localPosition = new Vector3(curPosX, curPosY, gyroGameObject.m_StartVec3.z);

        gyroGameObject.lastDiff_X = diff_X;
        gyroGameObject.lastDiff_Y = diff_Y;

        diff_X = 0;
        diff_Y = 0;
    }
    //旋转操作
    void RotateGameObject(GyroGameObject gyroGameObject)
    {
        if (gyroGameObject == null || gyroGameObject.go == null)
            return;
#if UNITY_EDITOR
        m_CurGyroX = valueGyroX;
#else
        m_CurGyroX = gyro.gravity.x;
#endif
        if (gyroGameObject.m_IsBack)
        {
            m_CurGyroX = 0 - m_CurGyroX;
        }
        float defY = gyroGameObject.m_StartVec3.y;
        float tagetY = m_CurGyroX * gyroGameObject.m_SpeedX + defY;
        if (tagetY < defY - gyroGameObject.m_RangeX)
        {
            tagetY = defY - gyroGameObject.m_RangeX;
        }
        else if (tagetY > defY + gyroGameObject.m_RangeX)
        {
            tagetY = defY + gyroGameObject.m_RangeX;
        }
        //插值计算平滑一点
        Quaternion taget = Quaternion.Euler(gyroGameObject.go.transform.localEulerAngles.x, tagetY, gyroGameObject.go.transform.localEulerAngles.z);
        gyroGameObject.go.transform.rotation = Quaternion.Slerp(gyroGameObject.go.transform.rotation, taget, Time.deltaTime * gyroGameObject.m_LerpParam);
    }
}
