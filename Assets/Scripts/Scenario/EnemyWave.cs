using UnityEngine;

//A wave is just an array of spawn sequences
[CreateAssetMenu]
public class EnemyWave : ScriptableObject
{
    [SerializeField]
    EnemySpawnSequence[] spawnSequences = {
        new EnemySpawnSequence()
    };

    public State Begin() => new State(this);


    //State contains the wave index and the state of the active sequence
    [System.Serializable]
    public struct State
    {

        EnemyWave wave;

        int index;

        EnemySpawnSequence.State sequence;

        public State(EnemyWave wave)
        {
            //Initialisation code
            this.wave = wave;
            index = 0;
            Debug.Assert(wave.spawnSequences.Length > 0, "Empty wave!");
            sequence = wave.spawnSequences[0].Begin();
        }

        public float Progress(float deltaTime)
        {
            deltaTime = sequence.Progress(deltaTime);
            //while the sequence isn't finished
            while(deltaTime >= 0f)
            {
                //No more waves to spawn
                if(++index >= wave.spawnSequences.Length)
                {
                    return deltaTime;
                }
                sequence = wave.spawnSequences[index].Begin();
                deltaTime = sequence.Progress(deltaTime);
            }
            return -1;
        }
    }
}
