using UnityEngine;

//Sequence needs to know which factory to use, which type of enemy to spawn, how many, and how quickly
[System.Serializable]
public class EnemySpawnSequence
{
    [SerializeField]
    EnemyFactory factory = default;

    [SerializeField]
    EnemyType type = EnemyType.Medium;

    [SerializeField, Range(1, 100)]
    int amount = 1;

    [SerializeField, Range(0.1f, 10f)]
    float cooldown = 1f;

    //Begin method that constructs the state and returns it
    public State Begin() => new State(this);

    //To progress through a scenario we have to keep track of its state
    //avoid memory allocations by making it as a struct instead of a class
    [System.Serializable]
    public struct State
    {
        int count;

        float cooldown;

        EnemySpawnSequence sequence;

        public State(EnemySpawnSequence sequence)
        {
            this.sequence = sequence;
            count = 0;
            cooldown = sequence.cooldown;
        }

        //Increases the cooldown by the time delta and then drops it back down if it reached the configured value
        public float Progress(float deltaTime)
        {
            cooldown += deltaTime;
            while (cooldown >= sequence.cooldown)
            {
                cooldown -= sequence.cooldown;
                //we must return the extra time at that point, to be used to progress the next sequence
                if (count >= sequence.amount)
                {
                    return cooldown;
                }
                count += 1;
                Game.SpawnEnemy(sequence.factory, sequence.type);
            }
            return -1f; 
        }
    }
}
