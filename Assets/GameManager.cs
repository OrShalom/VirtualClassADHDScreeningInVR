using Assets.Model;
using Newtonsoft.Json;
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
    public HeadTracker HeadTracker;
    string lettersData = "123456789123456789123456789";
    int lettersDelayInSec;
    float time;
    List<float> PressedAndshould;
    List<float> PressedAndshouldNot;
    List<float> NotPressedAndshould;
    QueueHandler startScreeningQueue;
    QueueHandler stopScreeningQueue;
    QueueHandler finishScreeningQueue;
    bool readMessage = false;
    bool stopGame = false;
    Patient patient;


    // Start is called before the first frame update
    void Start()
    {
        lettersDelayInSec = 1;
        InitQueueHandlers();
        RestartGame();
    }

    private void InitQueueHandlers()
    {
        startScreeningQueue = new QueueHandler("StartScreening");
        stopScreeningQueue = new QueueHandler("StopScreening");
        finishScreeningQueue = new QueueHandler("FinishScreening");
        startScreeningQueue.SubscribeToQueue((model, ea) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
            try
            {
                patient = JsonConvert.DeserializeObject<Patient>(message);

                Debug.Log($" [x] Received {message}");
                readMessage = true;
            }
            catch (Exception) { }
        });
        stopScreeningQueue.SubscribeToQueue((model, ea) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

            Debug.Log($" [x] Received {message}");
            stopGame = true;
        });
    }

    void RestartGame()
    {
        board.text = "Welcome to Virtual Classroom\nWaiting for the screening to start";
        readMessage = false;
        stopGame = false;
        patient = new Patient();
        PressedAndshould = new List<float>();
        PressedAndshouldNot = new List<float>();
        NotPressedAndshould = new List<float>();
        disturbances.Stop();
        HeadTracker.StopTracking();
    }

    // Update is called once per frame
    void Update()
    {
        if(readMessage)
        {
            board.text = "Press the button when you are ready!";
            button.ButtonPress.AddListener(StartGameAsync);
            readMessage = false;
        }
    }

    public void StartGameAsync()
    {
        button.ButtonPress.RemoveListener(StartGameAsync);
        StartCoroutine(GameCoroutine());
    }

    IEnumerator GameCoroutine()
    {
        Report report = new Report();
        report.Time = DateTime.Now;
        report.PatientId = patient.EmailAddress;
        bool pressed = false;
        button.ButtonPress.AddListener(() =>
        {
            pressed = true;
        });
        board.text = "Lets Start...";
        yield return new WaitForSecondsRealtime(4);
        disturbances.StartDisturbances(5f);
        HeadTracker.StartTracking();
        for (int i = 0; i < lettersData.Length; i++)
        {
            if(stopGame)
            {
                board.text = "Screening Stopped";
                yield return new WaitForSecondsRealtime(4);
                RestartGame();
                yield break;
            }
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
        report.HeadRotation = HeadTracker.StopTracking();
        report.PressedAndshould = PressedAndshould;
        report.NotPressedAndshould = NotPressedAndshould;
        report.PressedAndshouldNot= PressedAndshouldNot;
        finishScreeningQueue.SendMessageToQ(JsonConvert.SerializeObject(report));
        board.text = "Screening Has Finished";
        yield return new WaitForSecondsRealtime(3f);
        RestartGame();
        //string times = "";
        //foreach (var n in disturbances.TimesOfDisturbances)
        //    times += n + ", ";
        //board.text = times;
    }

    private bool ShouldPress(int i)
    {
        return i != 0 && lettersData[i] == 'x' && lettersData[i - 1] == 'a';
    }
}
