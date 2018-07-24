public class HeroCellDto
{
    public string Message;
    public int HeroId;
    public bool Unlocked;

    public HeroCellDto(int id, string msg, bool unlock)
    {
        HeroId = id;
        Message = msg;
        Unlocked = unlock;
    }
}