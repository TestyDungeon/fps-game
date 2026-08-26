using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private List<GameObject> defaultUIs;
    private List<GameObject> allUIs;

    void Awake()
    {
        allUIs = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            allUIs.Add(transform.GetChild(i).gameObject);
        }
    }

    void OnEnable()
    {
        foreach (GameObject ui in allUIs)
        {
            ui.SetActive(false);
        }
        foreach (GameObject ui in defaultUIs)
        {
            ui.SetActive(true);
        }
    }
}