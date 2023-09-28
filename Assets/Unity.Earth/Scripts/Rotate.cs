using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Transform target;//获取旋转目标
    public float rotateSpeed = 150;
    public float moveSpeed = 15;

    private void camerarotate() //摄像机围绕目标旋转操作
    {
        //transform.RotateAround(target.position, Vector3.up, speed * Time.deltaTime); //摄像机围绕目标旋转
        var mouse_x = Input.GetAxis("Mouse X");//获取鼠标X轴移动
        var mouse_y = -Input.GetAxis("Mouse Y");//获取鼠标Y轴移动
        //if (Input.GetKey(KeyCode.Mouse1))
        //{
        //    transform.Translate(Vector3.left * (mouse_x * 150f) * Time.deltaTime);
        //    transform.Translate(Vector3.up * (mouse_y * 150f) * Time.deltaTime);
        //}
        if (Input.GetMouseButton(1))
        {
            transform.RotateAround(target.transform.position, Vector3.up, mouse_x * rotateSpeed);
            transform.RotateAround(target.transform.position, transform.right, mouse_y * rotateSpeed);
        }
    }

    private void camerazoom() //摄像机滚轮缩放
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
            transform.Translate(Vector3.forward * moveSpeed);
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
            transform.Translate(Vector3.forward * moveSpeed);
    }

    private void Update()
    {
        camerarotate();
        camerazoom();
    }
}