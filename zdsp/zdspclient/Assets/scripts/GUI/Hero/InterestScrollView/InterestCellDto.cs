public class InterestCellDto
{
    public string Message;
    public byte Type;

    public InterestCellDto(byte type, string msg)
    {
        Type = type;
        Message = msg;
    }
}