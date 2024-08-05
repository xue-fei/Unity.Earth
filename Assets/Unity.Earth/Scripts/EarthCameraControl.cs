using System;
using System.Collections;
using UnityEngine;

public class EarthCameraControl : MonoBehaviour
{
    private float eulerAngles_x;
    private float eulerAngles_y;
    public EarthManager earthManager;
    public float SpeedRate;
    /// <summary>
    /// 距离
    /// </summary>
    public float distance;

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
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            //Vector3 back =Vector3.Normalize(  transform.position- Vector3.zero);
            transform.Translate(Vector3.back * Speed());

        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            Vector3 forward = Vector3.zero - transform.position;
            transform.Translate(Vector3.forward * Speed());
        }
    }

    float Speed()
    {
        //float B = (earthManager.EarthRadius) / (transform.position.magnitude - earthManager.EarthRadius); 
        //return SpeedRate/Mathf.Exp(B); 
        float B = (transform.position.magnitude - Earth.radius) / (Earth.radius);
        return SpeedRate * (B);
    }

    void PointRot()
    {
        this.eulerAngles_x = transform.eulerAngles.y;
        this.eulerAngles_y = transform.eulerAngles.x;
        float distancePoint = Vector3.Distance(transform.position, Vector3.zero);
        this.eulerAngles_x += (Input.GetAxis("Mouse X")) * Speed() * 0.01f;
        this.eulerAngles_y -= (Input.GetAxis("Mouse Y")) * Speed() * 0.01f;
        Quaternion quaternion = Quaternion.Euler(this.eulerAngles_y, this.eulerAngles_x, (float)0);
        Vector3 vector = ((Vector3)(quaternion * new Vector3((float)0, (float)0, -distancePoint)));
        transform.rotation = quaternion;
        transform.position = vector;
    }

    public void ToLocation(float lon, float lat, float time)
    {
        Quaternion from = Quaternion.Euler(transform.eulerAngles);
        Quaternion to = Quaternion.Euler(lat, 90 - lon, (float)0);
        float distancePoint = Vector3.Distance(transform.position, Vector3.zero);
        Debug.Log("distancePoint=" + distancePoint);
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
            Vector3 vector = ((Vector3)(quaternion * new Vector3((float)0, (float)0, -distancePoint)));
            transform.rotation = quaternion;
            transform.position = vector;
        }
    }
}