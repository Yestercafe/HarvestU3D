﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMPlay : DCLSingletonBase<BGMPlay>
{

    /// <summary>
    /// 调整 BGM 的音量
    /// </summary>
    /// <param name=“amount”> 取值范围 0f - 100f </param>
    public void SetVolume(float amount)
    {
        if (amount < 0) amount = 0;
        if (amount > 100) amount = 100;
        GetComponent<AudioSource>().volume = amount * .01f;
    }
}