﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour, IEntity
{
    // 转向速度，单位是 度每秒
    public float rotSpeed = 1f;
    public float maxRotZ = 30f;
    public float rotZScale = 10f;

    public float moveSpeed = 10f;

    // TODO 删除 cc
    private CharacterController cc;
    private Vector3 target;
    private string logInfo;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    public void SetTarget(Vector3 target_) => target = target_;

    void Update()
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        //     SetTargetOnPlane();
        // }
        TraceTarget();
        // HandleByUserInput();

        Vector3 deltaPos = transform.localToWorldMatrix * Vector3.forward * Time.deltaTime * moveSpeed;
        transform.position = transform.position + deltaPos;
        // transform.Translate(deltaPos);
        // cc.Move(deltaPos);
    }

    void TraceTarget()
    {
        var oldRot = transform.rotation;
        var rotZ = oldRot.eulerAngles.z;

        Vector3 local2TargetDir = (transform.worldToLocalMatrix * (target - transform.position)).normalized;

        float deltaRotY = 0f;
        float targetRotZ = 0f;
        if (local2TargetDir.x > Mathf.Epsilon)
        {
            deltaRotY = Time.deltaTime * rotSpeed;
            targetRotZ = -maxRotZ;
        }
        else if (local2TargetDir.x < -Mathf.Epsilon)
        {
            deltaRotY = -Time.deltaTime * rotSpeed;
            targetRotZ = maxRotZ;
        }

        float ModifyAngle(float dst)
        {
            if (dst > 180)
                return dst - 360;
            else if (dst < -180)
                return dst + 360;
            else
                return dst;
        }

        float ApproachAngle(float src, float dst, float absDelta)
        {
            dst = ModifyAngle(dst);
            src = ModifyAngle(src);

            if (dst - src < -Mathf.Epsilon)
                return Mathf.Max(dst, src - absDelta);
            else if (dst - src > Mathf.Epsilon)
                return Mathf.Min(dst, src + absDelta);
            else
                return src;
        }

        // rotZ = ApproachAngle(rotZ, targetRotZ, Time.deltaTime * rotZScale);
        float rotY = oldRot.eulerAngles.y + deltaRotY;
        if (Mathf.Abs(Mathf.DeltaAngle(rotZ, targetRotZ)) > Mathf.Epsilon)
            rotZ = Mathf.LerpAngle(rotZ, targetRotZ, Time.deltaTime * rotZScale);

        logInfo = $"RotZ {rotZ}\n RotY {rotY}\n deltaRotY {deltaRotY}\n {local2TargetDir}";
        transform.rotation = Quaternion.Euler(oldRot.x, rotY, rotZ);
    }

    /// <summary>
    /// 通过鼠标点击发出的射线，和一个具有碰撞体的物体的交点，来设置 targetpos
    /// </summary>
    void SetTargetOnPlane()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;
        if (Physics.Raycast(ray, out raycastHit, 50f))
        {
            SetTarget(raycastHit.point);
            Debug.DrawRay(ray.origin, raycastHit.point);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(target, 1f);
    }

    public void TakeDamage(float damage, GameObject attaker)
    {

    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 200, 80), logInfo);
    }
}