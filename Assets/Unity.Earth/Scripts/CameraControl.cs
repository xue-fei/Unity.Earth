using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
    public float speed = 150;
    /// <summary>
    /// 距离
    /// </summary>
    public float distance;
    /// <summary>
    /// 相机距海平面高度
    /// </summary>
    public float height;
    float distanceMin = 0.04f;
    float distanceMax = 16000f;
    public Vector3 currentEulerAngles;
    public Text text;

    // Start is called before the first frame update
    void Start()
    {
        //currentEulerAngles = transform.eulerAngles;
        PointRot();
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(Vector3.zero, transform.position);

        if (Input.GetMouseButton(0))
        {
            PointRot();
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && distance < distanceMax)
        {
            transform.Translate(Vector3.back * Speed());
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && distance - Earth.radius >= distanceMin)
        {
            transform.Translate(Vector3.forward * Speed());
        }
    }

    void LateUpdate()
    {
        height = distance - Earth.radius;
        text.text = "高度：" + (height * 10f).ToString("F2") + "千米";
    }

    float Speed()
    {
        float ratio = (transform.position.magnitude - Earth.radius) / (Earth.radius);
        return speed * (ratio);
    }

    void PointRot()
    {
        currentEulerAngles.z = 0;
        currentEulerAngles.x += Input.GetAxis("Mouse X") * Speed() * 0.01f;
        currentEulerAngles.y -= Input.GetAxis("Mouse Y") * Speed() * 0.01f;
        distance = Vector3.Distance(transform.position, Vector3.zero);
        Quaternion quaternion = Quaternion.Euler(currentEulerAngles.y, currentEulerAngles.x, 0);
        Vector3 vector = quaternion * new Vector3(0, 0, -distance);
        transform.rotation = quaternion;
        transform.position = vector;
    }

    public void ToLocation(float lon, float lat, float time)
    {
        Quaternion from = Quaternion.Euler(transform.eulerAngles);
        Quaternion to = Quaternion.Euler(lat, 90 - lon, 0);
        distance = Vector3.Distance(transform.position, Vector3.zero);
        Debug.Log("distancePoint = " + distance);
        StartCoroutine(DelayTime(move));
        IEnumerator DelayTime(Action<float> action)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / time;
                t = t > 1 ? 1 : t;
                action(t);
                yield return null;
            }
        }
        void move(float t)
        {
            Quaternion quaternion = Quaternion.Lerp(from, to, t);
            Vector3 vector = quaternion * new Vector3(0, 0, -distance);
            transform.rotation = quaternion;
            transform.position = vector;
            currentEulerAngles.x = transform.eulerAngles.y;
            currentEulerAngles.y = transform.eulerAngles.x;
        }
    }
}