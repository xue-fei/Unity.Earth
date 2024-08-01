using System;
using System.Collections;
using UnityEngine;

public class EarthCameraControl : MonoBehaviour
{
    private float eulerAngles_x;
    private float eulerAngles_y;
    // private float distancePoint;//绕点旋转距离
    public EarthManager earthManager;
    public float SpeedRate;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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

        float B = (transform.position.magnitude - earthManager.EarthRadius) / (earthManager.EarthRadius);
        return SpeedRate * (B);
    }

    void PointRot()
    {
        Vector3 eulerAngles = transform.eulerAngles;
        this.eulerAngles_x = eulerAngles.y;
        this.eulerAngles_y = eulerAngles.x;
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
            Debug.Log("distancePoint=" + distancePoint);
            Quaternion quaternion = Quaternion.Lerp(from, to, t);
            Vector3 vector = ((Vector3)(quaternion * new Vector3((float)0, (float)0, -distancePoint)));
            transform.rotation = quaternion;
            transform.position = vector;
        }
    }
}