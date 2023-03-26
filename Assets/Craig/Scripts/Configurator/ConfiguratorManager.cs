using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConfiguratorManager : MonoBehaviour
{
    [SerializeField] private PanelRoot _startPanel;
    [SerializeField] private TextMeshProUGUI _versionText;
    [SerializeField] private string _versionPre = "v";

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

        if (_versionText != null) _versionText.text = _versionPre + Application.version;

        
    }
}
