using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PanelRoot), true)]
public class PanelRootEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PanelRoot panel = (PanelRoot)target;

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Restore Buttons"))
            {
                panel.RestoreButtons();
            }
            if (GUILayout.Button("Unlock Buttons"))
            {
                panel.UnlockButtons();
            }

            
        }


    }
}
