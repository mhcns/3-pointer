using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField]
    private TMP_Text scoreText;

    [SerializeField]
    private float threePointDistance = 8.8f;

    private readonly Dictionary<Transform, HoopState> hoopStates = new();
    private int score;

    public event Action<int> ScoreChanged;

    private class HoopState
    {
        public ScoreTrigger topTrigger;
        public ScoreTrigger middleTrigger;
        public ScoreTrigger bottomTrigger;
        public ParticleSystem scoreParticles;
        public bool enteredTop;
        public bool enteredMiddle;
        public bool enteredBottom;
        public bool enteredFromBelow;
    }

    private void Awake()
    {
        Instance = this;
        UpdateScoreText();
    }

    public void TriggerEntered(ScoreTrigger trigger)
    {
        HoopState state = GetHoopState(trigger);

        switch (trigger.Zone)
        {
            case ScoreTrigger.TriggerZone.Top:
                if (!state.enteredFromBelow)
                {
                    state.enteredTop = true;
                    state.middleTrigger.SetTriggerEnabled(true);
                }
                break;

            case ScoreTrigger.TriggerZone.Middle:
                if (state.enteredTop)
                {
                    state.enteredMiddle = true;
                }
                break;

            case ScoreTrigger.TriggerZone.Bottom:
                if (!state.enteredTop)
                {
                    state.enteredFromBelow = true;
                    state.topTrigger.SetTriggerEnabled(false);
                    return;
                }

                if (state.enteredMiddle)
                {
                    state.enteredBottom = true;
                }
                break;
        }
    }

    public void TriggerExited(ScoreTrigger trigger, BallActions ball)
    {
        if (trigger.Zone != ScoreTrigger.TriggerZone.Bottom)
        {
            return;
        }

        HoopState state = GetHoopState(trigger);

        if (
            !state.enteredTop
            || !state.enteredMiddle
            || !state.enteredBottom
            || !ball.HasValidThrow
        )
        {
            return;
        }

        Vector2 throwPosition = new Vector2(ball.ThrowOrigin.x, ball.ThrowOrigin.z);
        Vector3 hoopPosition3D = state.topTrigger.transform.position;
        Vector2 hoopPosition = new Vector2(hoopPosition3D.x, hoopPosition3D.z);
        float distance = Vector2.Distance(throwPosition, hoopPosition);

        AddPoints(distance < threePointDistance ? 2 : 3);
        state.scoreParticles?.Play(true);
        state.scoreParticles?.GetComponent<AudioSource>()?.Play();
        ball.ConsumeThrow();
        ResetAllHoops();
    }

    public void ResetAllHoops()
    {
        foreach (HoopState state in hoopStates.Values)
        {
            ResetHoop(state);
        }
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreText();
        ResetAllHoops();
        ScoreChanged?.Invoke(score);
    }

    private HoopState GetHoopState(ScoreTrigger trigger)
    {
        Transform hoop = trigger.Hoop;

        if (hoopStates.TryGetValue(hoop, out HoopState state))
        {
            return state;
        }

        state = new HoopState();
        state.scoreParticles = hoop.GetComponentInChildren<ParticleSystem>(true);

        foreach (ScoreTrigger hoopTrigger in hoop.GetComponentsInChildren<ScoreTrigger>(true))
        {
            switch (hoopTrigger.Zone)
            {
                case ScoreTrigger.TriggerZone.Top:
                    state.topTrigger = hoopTrigger;
                    break;
                case ScoreTrigger.TriggerZone.Middle:
                    state.middleTrigger = hoopTrigger;
                    break;
                case ScoreTrigger.TriggerZone.Bottom:
                    state.bottomTrigger = hoopTrigger;
                    break;
            }
        }

        hoopStates.Add(hoop, state);

        if (state.scoreParticles != null)
        {
            state.scoreParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        ResetHoop(state);
        return state;
    }

    private void ResetHoop(HoopState state)
    {
        state.enteredTop = false;
        state.enteredMiddle = false;
        state.enteredBottom = false;
        state.enteredFromBelow = false;
        state.topTrigger.SetTriggerEnabled(true);
        state.middleTrigger.SetTriggerEnabled(false);
        state.bottomTrigger.SetTriggerEnabled(true);
    }

    private void AddPoints(int points)
    {
        score += points;
        UpdateScoreText();
        ScoreChanged?.Invoke(score);
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
}
