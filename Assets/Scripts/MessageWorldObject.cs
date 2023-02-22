using System;
using System.Collections;
using System.Collections.Generic;
using DBTables;
using TMPro;
using UnityEngine;


public class MessageWorldObject : MonoBehaviour
{
    public string UserID { get; private set; }
    public string Text { get; private set; }
    public DateTime DateCreated { get; private set; } = DateTime.Now;

    [SerializeField] TextMeshProUGUI textMesh;
    //[SerializeField] RectTransform canvas;


    private void Start()
    {
        if (textMesh == null)
            Debug.LogError("textMesh is missing a reference!");
    }

    //private void Update()
    //{
    //    if (canvas.gameObject.activeInHierarchy)
    //        canvas.LookAt(Camera.main.transform);
    //}


    public void Initialize(string userId, string text, DateTime dateCreated)
    {
        UserID = userId;
        DateCreated = dateCreated;

        Text = !string.IsNullOrEmpty(text) ? text : "N/A";
        textMesh.text = Text;
    }

    public void DisplayText()
    {
        textMesh.gameObject.SetActive(true);
    }

    public void HideText()
    {
        textMesh.gameObject.SetActive(false);
    }
}
