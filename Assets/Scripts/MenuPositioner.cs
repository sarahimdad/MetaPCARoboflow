using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPositioner : MonoBehaviour
{
    public float smoothFactor = 2;
    public Transform target;
    public Vector3 offset = new Vector3(0,.15f, .4f);
    public Vector3 euler = new Vector3(15, 0, 0);
    private void Start()
    {
        // Try to find target - first check if assigned, then try OVR camera rig, then fallback to main camera
        if (target == null)
        {
            // Try to find OVR camera rig (for VR/Quest)
            var ovrCameraRig = FindFirstObjectByType<OVRCameraRig>();
            if (ovrCameraRig != null && ovrCameraRig.centerEyeAnchor != null)
            {
                target = ovrCameraRig.centerEyeAnchor;
            }
            else
            {
                // Fallback to main camera
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    target = mainCam.transform;
                }
            }
        }
        
        if (target != null)
        {
            transform.position = GetTargetPos();
            transform.rotation = GetTargetRot();
        }
        else
        {
            Debug.LogWarning("MenuPositioner: No target camera found! Please assign a target or ensure Camera.main or OVRCameraRig exists.");
        }
    }
    void Update()
    {
        if (target == null) return;
        
        Vector3 targetPos = GetTargetPos();
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothFactor * Time.deltaTime);

        Quaternion targetRot = GetTargetRot();
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, smoothFactor * Time.deltaTime);
    }
    Vector3 GetTargetPos()
    {
        Vector3 targetPos = target.TransformPoint(offset);
        Vector3 forward = Vector3.ProjectOnPlane(target.forward, Vector3.up);
        targetPos = target.position + (forward * offset.z);
        targetPos.y = target.position.y - offset.y;
        return targetPos;
    }
    Quaternion GetTargetRot()
    {
        return Quaternion.Euler(euler.x, target.eulerAngles.y, euler.z);
    }
}

