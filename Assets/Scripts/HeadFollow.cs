using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadFollow : MonoBehaviour
{
    public Vector3 offset;
    public float smoothAmount = 1;
    private Transform head;
    // Start is called before the first frame update
    void Start()
    {
        // Try to find OVR camera rig (for VR/Quest)
        var ovrCameraRig = FindFirstObjectByType<OVRCameraRig>();
        if (ovrCameraRig != null && ovrCameraRig.centerEyeAnchor != null)
        {
            head = ovrCameraRig.centerEyeAnchor;
        }
        else
        {
            // Fallback to main camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                head = mainCam.transform;
            }
        }
    }
    private void OnEnable()
    {
        // Try to find OVR camera rig (for VR/Quest)
        var ovrCameraRig = FindFirstObjectByType<OVRCameraRig>();
        if (ovrCameraRig != null && ovrCameraRig.centerEyeAnchor != null)
        {
            head = ovrCameraRig.centerEyeAnchor;
        }
        else
        {
            // Fallback to main camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                head = mainCam.transform;
            }
        }
        
        if (head != null)
        {
            Vector3 targetPos = head.TransformPoint(offset);
            Quaternion targetRot = Quaternion.Euler(new Vector3(0, head.eulerAngles.y, 0));

            transform.position = targetPos;
            transform.rotation = targetRot;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (head == null) return;
        
        Vector3 targetPos = head.TransformPoint(offset);
        Quaternion targetRot = Quaternion.Euler(new Vector3(0, head.eulerAngles.y, 0));

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothAmount);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothAmount);
    }
}

