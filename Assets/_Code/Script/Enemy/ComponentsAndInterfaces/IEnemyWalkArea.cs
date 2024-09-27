namespace Ivayami.Enemy
{
    public interface IEnemyWalkArea
    {
        public bool CanChangeWalkArea { get; }
        public int ID { get; }
        public void SetWalkArea(EnemyWalkArea area);
        public void SetMovementData(EnemyMovementData data);
    }
}