using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintBox : MonoBehaviour
{
    public GameObject panel;
    
    public void OnPanel()
    {
        panel.SetActive(true);
    }

    public void OffPanel()
    {
        panel.SetActive(false);
    }

    public void Toggle()
    {
        panel.SetActive(!panel.activeSelf);
    }
}
