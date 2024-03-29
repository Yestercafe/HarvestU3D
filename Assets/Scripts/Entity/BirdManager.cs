﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BirdManager : EntityManager<Bird, BirdManager>
{
    [SerializeField] GameObject selectImage;
    [SerializeField] GameObject worldUICanvas;
    [SerializeField] Color birdSelectColor;
    [SerializeField] Color treeSelectColor;
    [SerializeField] Color lumbererSelectColor;
    [SerializeField] public float selectImageHeight;

    public int spawnCount = 10;
    public float spawnHeight = 2f;

    public float holdDownToRectSelect = 0.2f;

    // List<Bird> birds = new List<Bird>();
    [NonSerialized] public List<Bird> selectedBirds = new List<Bird>();

    private bool drawRectangle = false;
    private Vector3 rectStart, rectEnd;

    /// <summary>
    /// 当前鼠标悬浮所指的物体，假设同时不可能指向两个
    /// </summary>
    private GameObject curHover;
    /// <summary>
    /// 当前物体对应着的选中框
    /// </summary>
    [NonSerialized] public Dictionary<GameObject, Image> circlesOnObjects = new Dictionary<GameObject, Image>();
    /// <summary>
    /// tag 对应的 color，用于避免使用条件分支
    /// </summary>
    private Dictionary<string, Color> tag2Color;

    public bool Selecting => selectedBirds.Count > 0;

    void Start()
    {
        tag2Color = new Dictionary<string, Color>()
        {
            {"Bird", birdSelectColor},
            {"Tree", treeSelectColor},
            {"Lumberer", lumbererSelectColor},
        };
        // TestSpawnBirds();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            rectStart = Input.mousePosition;
            CameraRectDraw.I.drawRectangle = true;
            CameraRectDraw.I.rectStart = rectStart;
        }

        if (Input.GetMouseButtonUp(0))
        {
            CameraRectDraw.I.rectEnd = Input.mousePosition;
            RectSelect();
        }

        if (Input.GetMouseButtonDown(1))
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            BirdManager.I.SpawnBird(new Vector3(pos.x, spawnHeight, pos.z + 10));
            MainGameManager.I.IncNaturePower("SpawnBird");
        }

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;
        if (Physics.Raycast(ray, out raycastHit, 50f, LayerMask.GetMask("Tree", "Lumberer")))
        {
            if (Selecting && raycastHit.collider.gameObject.CompareTag("Tree"))
            {
                HoverObject(raycastHit.collider.gameObject);
            }
            else if (!Selecting && raycastHit.collider.gameObject.CompareTag("Lumberer"))
            {
                var lumb = raycastHit.collider.gameObject;
                HoverObject(lumb);
            }
        }
        else
        {
            UnHoverObject();
        }
    }

    void HoverObject(GameObject go)
    {
        UnHoverObject();
        curHover = go;
        SetImageOnObject(curHover);
    }

    void SetImageOnObject(GameObject go)
    {
        // Debug.Log("SetImageOnObject");
        Image image;
        if (!circlesOnObjects.TryGetValue(go, out image))
        {
            circlesOnObjects[go] = Instantiate(selectImage, worldUICanvas.transform).GetComponent<Image>();
            circlesOnObjects[go].GetComponent<SelectCircle>().setTrack(go);
            image = circlesOnObjects[go];
        }
        // [x] 更新位置
        // 在 SelectCircle.cs 中完成
        var pos = go.transform.position;
        pos.y = selectImageHeight;
        image.transform.position = pos;
        image.transform.rotation = Quaternion.Euler(90, 0, 0);

        image.enabled = true;
        image.color = tag2Color[go.tag];
    }

    void UnsetImageOnObject(GameObject go)
    {
        if (circlesOnObjects.TryGetValue(go, out var image))
        {
            image.enabled = false;
        }
    }

    void UnHoverObject()
    {
        if (curHover != null)
        {
            // Debug.Log("Unhover");
            var mesh = curHover.GetComponentInChildren<MeshRenderer>();
            UnsetImageOnObject(curHover);
            curHover = null;
        }
    }


    void ClickSelect()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;
        bool selectAction = false;
        if (Physics.Raycast(ray, out raycastHit, 50f, LayerMask.GetMask("Bird", "Tree", "Lumberer")))
        {
            Debug.Log($"Hit a object {raycastHit.transform.name}");
            var hitCol = raycastHit.collider;
            switch (hitCol.tag)
            {
                case "Bird":
                    {
                        Debug.Log($"Selected a bird, {hitCol.name}");
                        DoSelectBird(hitCol.gameObject);
                        selectAction = true;
                        break;
                    }
                case "Lumberer":
                    Debug.Log($"Set target to {hitCol.name} at {raycastHit.point}");
                    foreach (var bird in selectedBirds)
                    {
                        bird.SetTarget(raycastHit.point);
                    }
                    UnHoverObject();
                    break;
                case "Tree":
                    Debug.Log($"Set target to {hitCol.name} at {raycastHit.point}");
                    foreach (var bird in selectedBirds)
                    {
                        bird.flyToTree = true;
                        bird.SetTarget(raycastHit.point);
                    }
                    UnHoverObject();
                    break;
                default:
                    break;
            }
            Debug.DrawRay(ray.origin, raycastHit.point);
        }

        if (!selectAction)
        {
            UnSelectAll();
        }
    }

    void RectSelect()
    {
        CameraRectDraw.I.drawRectangle = drawRectangle = false;
        rectEnd = CameraRectDraw.I.rectEnd;
        Debug.Log($"rectEnd = {rectEnd}, rectStart = {rectStart}");
        if (Vector3.Distance(rectEnd, rectStart) < 10f)    // 选框过小就算点击
        {
            ClickSelect();
            return;
        }
        Vector3 p1 = Vector3.zero;
        Vector3 p2 = Vector3.zero;
        if (rectStart.x > rectEnd.x)
        {
            // 这些判断是用来确保 p1 的 xy 坐标小于 p2 的 xy 坐标，因为画的框不见得就是左下到右上这个方向的
            p1.x = rectEnd.x;
            p2.x = rectStart.x;
        }
        else
        {
            p1.x = rectStart.x;
            p2.x = rectEnd.x;
        }

        if (rectStart.y > rectEnd.y)
        {
            p1.y = rectEnd.y;
            p2.y = rectStart.y;
        }
        else
        {
            p1.y = rectStart.y;
            p2.y = rectEnd.y;
        }

        foreach (var obj in entitys)
        {
            Vector3 location = Camera.main.WorldToScreenPoint(obj.transform.position); // 把对象的 position 转换成屏幕坐标
            if (location.x < p1.x || location.x > p2.x || location.y < p1.y || location.y > p2.y
            || location.z < Camera.main.nearClipPlane || location.z > Camera.main.farClipPlane) //z 方向就用摄像机的设定值，看不见的也不需要选择了
            {
                UnSelectBird(obj);
            }
            else
            {
                SelectBird(obj);
            }
        }
    }



    #region Selection

    void DoSelectBird(GameObject go)
    {
        var bird = go.GetComponent<Bird>();
        if (selectedBirds.Contains(bird))
        {
            UnSelectBird(bird);
        }
        else
        {
            SelectBird(bird);
        }
    }

    void SelectBird(GameObject go) => SelectBird(go.GetComponent<Bird>());
    void SelectBird(Bird bird)
    {
        SetImageOnObject(bird.gameObject);
        selectedBirds.Add(bird);
    }

    void UnSelectBird(Bird bird)
    {
        UnsetImageOnObject(bird.gameObject);
        selectedBirds.Remove(bird);
    }
    void UnSelectAll()
    {
        foreach (var bird in selectedBirds)
        {
            UnsetImageOnObject(bird.gameObject);
        }
        selectedBirds.Clear();
    }

    #endregion

    void TestSpawnBirds()
    {
        while (entitys.Count < spawnCount)
        {
            SpawnBird(Helper.RandomOnCircle(new Vector3(0, spawnHeight, 0), 2f));
        }
    }

    void SpawnBird(Vector3 pos)
    {
        Bird newBird = CreateEntity(pos);
        newBird.gameObject.GetComponent<Animator>().SetBool("flying", true);
    }
}
