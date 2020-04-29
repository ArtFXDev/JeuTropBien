using UnityEngine;

//A gameplay scenario is created from a sequence of waves.
[CreateAssetMenu]
public class GameScenario : ScriptableObject
{
    //Values
    [SerializeField]
    EnemyWave[] waves = { };

    //Number of time the scenario will repeat, 0 = infinite
    [SerializeField, Range(0, 10)]
    int cycles = 1;

    //Control the speed-up per cycle (by default it will ad 50% of speed everywave)
    [SerializeField, Range(0f, 1f)]
    float cycleSpeedUp = 0.5f;

    //--------------------------------------------------------
    //Functions

    public State Begin() => new State(this);

    //State contains the wave index and the active wave state, also keep track of the cycle number
    [System.Serializable]
    public struct State
    {
        GameScenario scenario;

        int cycle, index;

        float timeScale;

        EnemyWave.State wave;

        public State(GameScenario scenario)
        {
            this.scenario = scenario;
            cycle = 0;
            index = 0;
            timeScale = 1;
            Debug.Assert(scenario.waves.Length > 0, "Empty scenario!");
            wave = scenario.waves[0].Begin();
        }

        //Top level, Progress method doesn't require parameter, we can directly use Time.deltaTime
        //We don't need to return remaining time, but do need to indicate whether the scenario is finished or not
        public bool Progress()
        {
            float deltaTime = wave.Progress(timeScale * Time.deltaTime);
            while (deltaTime >= 0f)
            {
                //Check if we've finished the scenario
                if(++index >= scenario.waves.Length)
                {
                    if(++cycle > scenario.cycles && scenario.cycles > 0)
                    {
                        return false;
                    }
                    index = 0;
                    timeScale += scenario.cycleSpeedUp;
                }
                wave = scenario.waves[index].Begin();
                deltaTime = wave.Progress(deltaTime);
            }
            //Scenario not finished
            return true;
        }
    }
}
