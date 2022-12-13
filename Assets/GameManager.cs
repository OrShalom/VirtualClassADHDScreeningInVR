using System;
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
    int lettersDelay;
    float time;
    List<float> PressedAndshould;
    List<float> PressedAndshouldNot;
    List<float> NotPressedAndshould;



    // Start is called before the first frame update
    void Start()
    {
        lettersDelay = 1000;
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
        await Task.Delay(4000);
        disturbances.StartDisturbances(1f);
        bool pressed = false;
        button.ButtonPress.AddListener(() =>
        {
            pressed = true;
        });
        for (int i = 0; i < lettersData.Length; i++)
        {
            pressed = false;
            board.text = lettersData[i].ToString();
            await Task.Delay(lettersDelay);
            if (pressed && ShouldPress(i))
            {
                time = (i * lettersDelay) / (60 / 1000);
                PressedAndshould.Add(time);
            }
            if (pressed && !ShouldPress(i))
            {
               time = (i * lettersDelay) / (60 / 1000);
                PressedAndshouldNot.Add(time);
            }
            if(!pressed && ShouldPress(i))
            {
                time = (i * lettersDelay) / (60 / 1000);
                NotPressedAndshould.Add(time);
            }


        }
        disturbances.Stop();
        board.text = "U R DONE";
        await Task.Delay(2000);
        string times = "";
        foreach (var n in disturbances.TimesOfDisturbances)
            times += n + ", ";
        board.text = times;
    }

    private bool ShouldPress(int i)
    {
        return i != 0 && lettersData[i] == 'x' && lettersData[i - 1] == 'a';
    }

    IEnumerator WaitFor(int seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}
