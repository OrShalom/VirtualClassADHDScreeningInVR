using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI board;
    public ButtonVR button;
    public DisturbancesManager disturbances;
    string lettersData = "vcda";

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
        await Task.Delay(2000);
        disturbances.StartDisturbances(1f);
        foreach (char c in lettersData)
        {
            board.text = c.ToString();
            await Task.Delay(1000);
        }
        disturbances.Stop();
        board.text = "U R DONE";
        await Task.Delay(2000);
        string times = "";
        foreach (var n in disturbances.TimesOfDisturbances)
            times += n + ", ";
        board.text = times;
    }


    IEnumerator WaitFor(int seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}
