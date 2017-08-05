using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Extensions;

public class TimeControl : MonoBehaviour
{
    private Text txtFPS, txtSpeed;

    private Button btnUp, btnDown, btnTime, btnPause;

    int _frameCounter = 0;
    float _timeCounter = 0.0f;
    //?float _lastFramerate = 0.0f;
    public float _refreshTime = 0.5f; //Refresh FPS every .5 seconds
    
    private bool _drawTime = true;
    private float previousTimeScale;
    private float[] timeScaleOptions;
    private int currentOption;

    void Start()
    {
        timeScaleOptions = new float[] {
            0.02f, 0.1f, 0.3f, 0.5f, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 25, 30, 40, 50
        };
        currentOption = 4;

        txtFPS = this.TextByName("txtFPS");
        txtSpeed = this.TextByName("txtSpeed");

        btnUp = this.ButtonByName("btnUp");
        btnDown = this.ButtonByName("btnDown");

        btnUp.SetText("+");
        btnDown.SetText("-");

        btnUp.onClick.AddListener(btnUp_Click);
        btnDown.onClick.AddListener(btnDown_Click);

        btnTime = this.ButtonByName("btnTime");
        btnTime.onClick.AddListener(btnTime_Click);

        btnPause = this.ButtonByName("btnPause");
        btnPause.SetText("||");
        btnPause.onClick.AddListener(btnPause_Click);
    }

    private void btnPause_Click()
    {
        if (Time.timeScale != 0)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            btnPause.SetText("|>");
        }
        else
        {
            Time.timeScale = previousTimeScale;
            btnPause.SetText("||");
        }
    }

    private void btnTime_Click()
    {
        _drawTime = !_drawTime;
    }

    private void btnDown_Click()
    {
        ModifySpeed(-1);
    }

    private void btnUp_Click()
    {
        ModifySpeed(1);
    }

    private void ModifySpeed(int change)
    {
        currentOption += change;
        currentOption = Mathf.Clamp(currentOption, 0, timeScaleOptions.Length);

        float newSpeed = timeScaleOptions[currentOption];

        txtSpeed.text = newSpeed + "x";
        Time.timeScale = newSpeed;
    }

    void Update()
    {
        if (SimulationManager.Instance != null)
        {
            UpdateFPS();
        }
    }

    void FixedUpdate()
    {
        if (SimulationManager.Instance != null)
            UpdateTime();
    }

    private void UpdateTime()
    {
        if (_drawTime)
        {
            var time = SimulationManager.Instance.TotalElapsedSimulatedTime("s");
            btnTime.SetText(System.TimeSpan.FromSeconds(time).ToOutputString());
        }
        else
        {
            btnTime.SetText(SimulationManager.Instance.currentTick.ToString());
        }
    }

    private void UpdateFPS()
    {
        if (_timeCounter < _refreshTime)
        {
            _timeCounter += Time.deltaTime;
            _frameCounter++;
        }
        else
        {
            txtSpeed.text = Time.timeScale + "x";
            txtFPS.text = string.Format("FPS: {0}", _frameCounter);
            //?_lastFramerate = (float)_frameCounter / _timeCounter;
            _frameCounter = 0;
            _timeCounter = 0.0f;
        }
    }
}
