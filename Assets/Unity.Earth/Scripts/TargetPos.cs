using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetPos : MonoBehaviour
{
    public EarthCameraControl cameraControl;
    public EarthManager earthManager;
    public InputField lat;
    public InputField lon;
    public Transform prop;
    public void CameraToTarget()
    {
        cameraControl.ToLocation( float.Parse(lon.text), float.Parse(lat.text), 2);
        prop.position= earthManager.ToLocationPos(float.Parse(lon.text), float.Parse(lat.text));
    }
}
