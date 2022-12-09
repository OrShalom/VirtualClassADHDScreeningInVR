using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CounterScript : MonoBehaviour
{
    int counter;
    public TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        counter = 0;
        text.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        counter++;
        if (counter % 20 == 0)
        {
            text.text = (int.Parse(text.text) + 1).ToString();
        }
    }
}
