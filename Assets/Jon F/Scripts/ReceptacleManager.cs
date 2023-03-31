using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReceptacleManager : MonoBehaviour
{
    public uint sumB = 0;
    public uint sumY = 0;
    public uint sumR = 0;
    public TextMeshProUGUI sumTxtB;
    public TextMeshProUGUI sumTxtY;
    public TextMeshProUGUI sumTxtR;

    //public List<int> plantsReceived = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        UpdateText();
    }

    // Update is called once per frame
    private void UpdateText()
    {
        sumTxtB.text = ((uint)sumB).ToString();
        sumTxtR.text = ((uint)sumR).ToString();
        sumTxtY.text = ((uint)sumY).ToString();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "blueplant")
        {
            sumB++;
            Destroy(collision.gameObject);
            //Debug.Log("Collision detected");
        }
        if (collision.gameObject.tag == "redplant")
        {
            sumR++;
            Destroy(collision.gameObject);
            //Debug.Log("Collision detected");

        }
        if (collision.gameObject.tag == "yellowplant")
        {
            sumY++;
            Destroy(collision.gameObject);
            //Debug.Log("Collision detected");

        }
        UpdateText();
    }

}
