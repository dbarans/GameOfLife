using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject MenuPanel;


    private void Start()
    {
        MenuPanel.SetActive(false);
    }

    public void Show()
    {
        Debug.Log("Showing Menu Panel");
        MenuPanel.SetActive(true);
    }

    public void Hide()
    {
        Debug.Log("Hiding Menu Panel");
        MenuPanel.SetActive(false);
    }
}

