namespace Ivayami.Enemy
{
    public interface IEnemyWalkArea
    {
        public int ID { get; }
        public bool SetWalkArea(EnemyWalkArea area);
        public void SetMovementData(EnemyMovementData data);
    }
}