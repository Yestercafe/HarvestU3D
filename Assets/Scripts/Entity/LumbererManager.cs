﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LumbererManager : EntityManager<Lumberer, LumbererManager>
{
    public float lumbererHeight = 0.45f;
    Lumberer lumberer;

    // minDist 的说明见 Lumberer::SetDestination::modDist
    public float minDist = .3f;
    public float distToOuterTree;
    public int spawnCount = 1;
    public float spawnCD = 1f;

    void Start()
    {
        ArrangeChild();
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        var spawnRadius = distToOuterTree + Trees.I.radius;
        while (entitys.Count < spawnCount)
        {
            var lumb = CreateEntity(Helper.RandomOnCircle(Vector3.up * lumbererHeight, spawnRadius));
            SetTargetTree4Lumberer(lumb);
            yield return new WaitForSeconds(spawnCD);
        }
        yield return null;
    }

    public void SetTargetTree4Lumberer(Lumberer lumberer)
    {
        Tree closestTree = Trees.I.GetClosestTree(lumberer.transform.position);
        if (closestTree != null)
        {
            lumberer.SetTargetTree(closestTree, minDist);
            Trees.I.RemoveTree(closestTree);      // 将已经被占用的树从 KDTree 队列中删除
        }
    }

#if USER_CONTROL
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("Attack 1 trig");
            lumberer.Attack1();
        }

        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");

        if (!GetComponent<Lumberer>().isAttacking)
        {
            lumberer.Walk(new Vector3(h, 0f, v));

            var lookAt = Vector3.forward * v + Vector3.right * h;
            if (lookAt.magnitude != 0)
            {
                lumberer.Face(lookAt);
            }
        }
        else
            lumberer.Stop();
    }
#endif

}
