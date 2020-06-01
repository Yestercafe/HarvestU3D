﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour, IEntity
{
    public float health = 10f;

    private GameObject treeStump;
    private bool isDead = false;


    private Vector3 deathDeltaPos;
    private float deathRotationTheta;

    private void Start()
    {
        treeStump = Trees.I.treeStump;
    }

    private void Update()
    {
        if (isDead)
        {
            transform.Rotate(new Vector3(Mathf.Sin(deathRotationTheta), 0f, Mathf.Cos(deathRotationTheta)) * Time.deltaTime);
            if (transform.eulerAngles.x >= 90f)
                Destroy(gameObject);
        }
    }
    public void TakeDamage(float damage, GameObject attaker)
    {
        health -= damage;
        if (health <= 0f)
        {
            Death(attaker);
        }
    }
    
    void Death(GameObject murderer)
    {
        DeathAnimation();

        var position = transform.position;
        var rotation = transform.rotation;
        Instantiate(treeStump, position, rotation, Trees.I.transform);

        deathDeltaPos = murderer.transform.position - this.transform.position;
        deathRotationTheta = (float) Math.Tanh(deathDeltaPos.z / deathDeltaPos.x) + 90f;
        Debug.Log($"{name} deathRotationTheta = {deathRotationTheta}");

        Trees.I.RemoveTree(this);
        isDead = true;
    }

    void DeathAnimation()
    {

    }
    
}
