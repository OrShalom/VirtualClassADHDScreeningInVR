using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI board;
    public ButtonVR button;
    public DisturbancesManager disturbances;
    public GameObject vrcam;
    string lettersData = "123456789123456789123456789";
    int lettersDelayInSec;
    float time;
    List<float> PressedAndshould;
    List<float> PressedAndshouldNot;
    List<float> NotPressedAndshould;
    QueueHandler startScreeningQueue;



    // Start is called before the first frame update
    void Start()
    {
        startScreeningQueue = new QueueHandler("StartScreening");
        startScreeningQueue.SubscribeToQueue((model, ea) =>
        {
            var body = ea.Body;
            var message = System.Text.Encoding.UTF8.GetString(body.ToArray());

            Debug.Log($" [x] Received {message}");
        });
        lettersDelayInSec = 1;
        board.text = "Waiting for the screening to start";
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
        StartCoroutine(LettersCoroutine());
    }

    IEnumerator LettersCoroutine()
    {
        disturbances.StartDisturbances(5f);
        bool pressed = false;
        button.ButtonPress.AddListener(() =>
        {
            pressed = true;
        });
        for (int i = 0; i < lettersData.Length; i++)
        {
            pressed = false;
            board.text = lettersData[i].ToString();
            yield return new WaitForSecondsRealtime(lettersDelayInSec);
            if (pressed && ShouldPress(i))
            {
                time = (i * lettersDelayInSec) / (60 / 1000);
                PressedAndshould.Add(time);
            }
            if (pressed && !ShouldPress(i))
            {
                time = (i * lettersDelayInSec) / (60 / 1000);
                PressedAndshouldNot.Add(time);
            }
            if (!pressed && ShouldPress(i))
            {
                time = (i * lettersDelayInSec) / (60 / 1000);
                NotPressedAndshould.Add(time);
            }
        }
        disturbances.Stop();
        board.text = "U R DONE";
        yield return new WaitForSecondsRealtime(2f);
        string times = "";
        foreach (var n in disturbances.TimesOfDisturbances)
            times += n + ", ";
        board.text = times;
    }

    private bool ShouldPress(int i)
    {
        return i != 0 && lettersData[i] == 'x' && lettersData[i - 1] == 'a';
    }
}
