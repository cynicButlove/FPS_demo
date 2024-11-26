using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    private float mouseSenstivity;//���������
    private Transform playerBody;
    public float yRotation;//�����������ת
    private float xRotation;//�����������ת
    private float originalYRotation;

    private CharacterController characterController;
    private float height;
    private float interpolationSpeed = 12f;



    // Start is called before the first frame update
    void Start()
    {
        mouseSenstivity = 400f;
        yRotation = 0f;
        playerBody = transform.GetComponentInParent<PlayerControl>().transform;
        characterController = GetComponentInParent<CharacterController>();
        height= characterController.height;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSenstivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSenstivity * Time.deltaTime;
        
        yRotation -= mouseY;
        yRotation = Mathf.Clamp(yRotation, -60f, 60f);//�޶���ת��Χ

        //transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);//������ת�����(x��)��localRotation������ڸ��������ת
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            //����ԭʼ�� Y ����ת�Ƕ�
            originalYRotation = yRotation;
        }
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            xRotation += mouseX;
            xRotation = Mathf.Clamp(xRotation, -120f, 120f);//�޶���ת��Χ
            transform.localRotation = Quaternion.Euler(yRotation, xRotation, 0f);//������ת�����(y��)
        }
        else
        {
            xRotation = Mathf.Lerp(xRotation, 0f, interpolationSpeed * Time.deltaTime);//����
            transform.localRotation = Quaternion.Euler(yRotation, xRotation, 0f);//������ת�����(x��--yRotatio),������ת�����(y��--xRotation)
            playerBody.Rotate(Vector3.up * mouseX);//���������ת(y��)��ͬʱҲ���������һ����ת,Rotate�ı������������ת
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            yRotation=originalYRotation;
            //transform.localRotation = Quaternion.Euler(new Vector3( originalYRotation, 0f, 0f));//����ͷ���»���
        }


        float heightTarget = characterController.height * 0.9f;
        height=Mathf.Lerp(height,heightTarget,interpolationSpeed*Time.deltaTime);//����
        transform.localPosition = Vector3.up * height;//�������λ��

    }
}
