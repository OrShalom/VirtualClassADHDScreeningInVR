using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI board;
    public ButtonVR button;
    string lettersData = "vcdaxfgaxgbbxax";

    // Start is called before the first frame update
    void Start()
    {
        board.text = "Welcome to Virtual Classroom\n Press the button when you are ready!";
        button.ButtonPress.AddListener(StartGameAsync);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void StartGameAsync()
    {
        button.ButtonPress.RemoveListener(StartGameAsync);
        board.text = "Lets Start...";
        //StartCoroutine(WaitFor(5));
        await Task.Delay(5000);
        board.text = "NOW";
        await Task.Delay(1000);
        foreach(char c in lettersData)
        {
            board.text = c.ToString();
            await Task.Delay(3000);
        }


    }


    IEnumerator WaitFor(int seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}
