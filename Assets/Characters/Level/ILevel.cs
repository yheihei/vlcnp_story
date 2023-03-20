public interface ILevel
{
    public int Level { get; }
    public int MaxLevel { get; }
    public int Experience { get; set; }
    void AddExperience(int point);
    void LoseExperience(int point);
}