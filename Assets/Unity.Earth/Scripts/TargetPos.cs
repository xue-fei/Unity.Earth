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
        cameraControl.ToLocation(float.Parse(lon.text), float.Parse(lat.text), 2);
        prop.position = Earth.ToLocationPos(float.Parse(lon.text), float.Parse(lat.text));
        //prop.position = earthManager.GetSphericalCoordinates(double.Parse(lon.text), double.Parse(lat.text));
    }
}