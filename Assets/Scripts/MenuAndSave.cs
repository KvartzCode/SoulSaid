using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuAndSave : MonoBehaviour
{
    [SerializeField] Slider colorSlider;
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] Button playButton;

    string name;
    float color;


    void Start()
    {
        name = PlayerPrefs.GetString("PlayerName", "DefaultName");
        nameInput.onValueChanged.AddListener(delegate { Test(); });
    }

    void Test()
    {
        PlayerPrefs.SetString("PlayerName", nameInput.text);
    }
}
