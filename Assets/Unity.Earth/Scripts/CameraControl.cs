using System;
using System.Collections;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private float eulerAngles_x;
    private float eulerAngles_y;
    public float speed = 150;
    /// <summary>
    /// 距离
    /// </summary>
    public float distance;

    float distanceMin = 0.1f;
    float distanceMax = 16000f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(Vector3.zero, transform.position);
        if (Input.GetMouseButton(1))
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

    float Speed()
    {
        float ratio = (transform.position.magnitude - Earth.radius) / (Earth.radius);
        return speed * (ratio);
    }

    void PointRot()
    {
        eulerAngles_x = transform.eulerAngles.y;
        eulerAngles_y = transform.eulerAngles.x;
        distance = Vector3.Distance(transform.position, Vector3.zero);
        eulerAngles_x += (Input.GetAxis("Mouse X")) * Speed() * 0.01f;
        eulerAngles_y -= (Input.GetAxis("Mouse Y")) * Speed() * 0.01f;
        Quaternion quaternion = Quaternion.Euler(eulerAngles_y, eulerAngles_x, 0);
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
        }
    }
}