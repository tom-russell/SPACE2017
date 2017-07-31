using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Extensions;

public class TimeControl : MonoBehaviour
{
    private Text txtFPS, txtSpeed;

    private Button btnUp, btnDown, btnTick, btnTime, btnPause;

    int _frameCounter = 0;
    float _timeCounter = 0.0f;
    //?float _lastFramerate = 0.0f;
    public float _refreshTime = 0.5f; //Refresh FPS every .5 seconds

    private int _currentSpeed = 1;
    private bool _drawTime = true;
    private float previousTimeScale;

    void Start()
    {
        txtFPS = this.TextByName("txtFPS");
        txtSpeed = this.TextByName("txtSpeed");

        btnUp = this.ButtonByName("btnUp");
        btnDown = this.ButtonByName("btnDown");

        btnUp.SetText("+");
        btnDown.SetText("-");

        btnUp.onClick.AddListener(btnUp_Click);
        btnDown.onClick.AddListener(btnDown_Click);

        btnTick = this.ButtonByName("btnTick");
        btnTick.SetText("Tick");
        btnTick.onClick.AddListener(btnTick_Click);

        btnTime = this.ButtonByName("btnTime");
        btnTime.onClick.AddListener(btnTime_Click);

        btnPause = this.ButtonByName("btnPause");
        btnPause.onClick.AddListener(btnPause_Click);
    }

    private void btnPause_Click()
    {
        if (Time.timeScale != 0)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            UpdatePause(true);
        }
        else
        {
            Time.timeScale = previousTimeScale;
            UpdatePause(false);
        }
    }

    private void UpdatePause(bool paused)
    {
        btnPause.SetText(paused ? "|>" : "||");
    }

    private void btnTime_Click()
    {
        _drawTime = !_drawTime;
    }

    private void btnTick_Click()
    {
        SimulationManager.Instance.TickManager.TickOnce = true;
    }

    private void btnDown_Click()
    {
        ModifySpeed(-1);
    }

    private void btnUp_Click()
    {
        ModifySpeed(1);
    }

    private void ModifySpeed(int by)
    {
        int newSpeed = _currentSpeed += by;

        if (newSpeed < 0)
            newSpeed = 0;

        _currentSpeed = newSpeed;

        txtSpeed.text = _currentSpeed + "x";

        Time.timeScale = _currentSpeed;
        //SimulationManager.Instance.TickManager.TicksPerFrame = _currentSpeed;
    }

    void Update()
    {
        if (SimulationManager.Instance != null)
        {
            UpdateFPS();
            UpdatePause(SimulationManager.Instance.TickManager.IsPaused);
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
            var time = SimulationManager.Instance.TickManager.TotalElapsedSimulatedTime;

            btnTime.SetText(time.ToOutputString());
        }
        else
        {
            btnTime.SetText(SimulationManager.Instance.TickManager.CurrentTick.ToString());
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
            txtSpeed.text = SimulationManager.Instance.TickManager.TicksPerFrame.ToString() + "x";
            txtFPS.text = string.Format("FPS: {0}", _frameCounter);
            //?_lastFramerate = (float)_frameCounter / _timeCounter;
            _frameCounter = 0;
            _timeCounter = 0.0f;
        }
    }
}
