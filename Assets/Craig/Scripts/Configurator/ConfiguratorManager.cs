using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfiguratorManager : MonoBehaviour
{
    [SerializeField] private PanelRoot _startPanel;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void Awake()
    {
        List<PanelRoot> panelList = new List<PanelRoot>();

        GetComponentsInChildren<PanelRoot>(panelList);

        foreach(PanelRoot panel in panelList)
        {
            if (panel != _startPanel) panel.gameObject.SetActive(false);
        }
    }
}
