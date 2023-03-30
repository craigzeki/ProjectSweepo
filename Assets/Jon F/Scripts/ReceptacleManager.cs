using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReceptacleManager : MonoBehaviour
{
    public uint sumB;
    public uint sumY;
    public uint sumR;
    public TextMeshProUGUI sumTxtB;
    public TextMeshProUGUI sumTxtY;
    public TextMeshProUGUI sumTxtR;

    //public List<int> plantsReceived = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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
            Debug.Log("Collision detected");
        }
        if (collision.gameObject.tag == "redplant")
        {
            sumR++; Debug.Log("Collision detected");

        }
        if (collision.gameObject.tag == "yellowplant")
        {
            sumY++;
            Debug.Log("Collision detected");

        }
        else return;
    }

}
