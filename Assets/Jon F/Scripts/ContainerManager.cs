using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContainerManager : MonoBehaviour
{

    [SerializeField] Transform shelfTransform;
    [SerializeField] public bool isLocked = true;
    [SerializeField] Toggle buttonToggle;
    private bool shelfMoving = false;
    [SerializeField]float lerpDuration = 2.5f;
    float startValue = 0;
    [SerializeField]float endValue = 10;
    Vector3 lerpPos;

    private void Awake()
    {
        if (shelfTransform == null) return;
        startValue = shelfTransform.localPosition.z;
        lerpPos = shelfTransform.localPosition;
    }
    IEnumerator DoShelfMove(bool isOpening, Toggle toggle)
    {
        shelfMoving = true;
        toggle.interactable = false;
        float timeElapsed = 0;
        while (timeElapsed < lerpDuration)
        {
            if (isOpening)
            {
                lerpPos.z = Mathf.Lerp(startValue, endValue, timeElapsed / lerpDuration);
            }
            else
            {
                lerpPos.z = Mathf.Lerp(endValue, startValue, timeElapsed / lerpDuration);
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        if(isOpening)
        {
            lerpPos.z = endValue;
        }
        else
        {
            lerpPos.z = startValue;
        }
        shelfMoving = false;
        toggle.interactable = true;
    }

    // Update is called once per frame
    void Update()
    {
        shelfTransform.localPosition = lerpPos;
    }
    private void OnGUI()
    {
        if (buttonToggle == null) return;
        buttonToggle.interactable = !isLocked && !shelfMoving;
    }

    public void buttonPressed(Toggle toggle)
    {
        if (isLocked) return;
        if (buttonToggle == null) return;
        if (shelfTransform == null) return;
        if (shelfMoving) return;

        

        StartCoroutine(DoShelfMove(buttonToggle.isOn, toggle));
    }
}
