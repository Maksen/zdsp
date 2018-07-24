public class KopioThreadJob : ThreadedJob
{
    public string gameText;
    protected override void ThreadFunction()
    {
        Zealot.Repository.GameRepo.SetItemFactory(new Zealot.Repository.ClientItemFactory());
        Zealot.Repository.GameRepo.InitClient(gameText);
    }
}

//public class GameLoadingThreadJob : ThreadedJob
//{
//    public Action threadFn;
//    protected override void ThreadFunction()
//    {
//        if (threadFn != null)
//            threadFn.Invoke();
//    }
//}

//public class GameLoadThreadJob : ThreadedJob
//{
//    public enum ErrCode
//    {
//        NUM,
//    }
//    public enum PhaseCode
//    {
//        NUM,
//    }
//    private object progress_Handle = new object();
//    private object errorCode_Handle = new object();
//    private object phaseCode_Handle = new object();

//    private float m_progress = 0.0f;
//    private ErrCode m_errorCode = ErrCode.NUM;
//    private PhaseCode m_phaseCode = PhaseCode.NUM;

//    //shared variable
//    public float progress {
//        get
//        {
//            float tmp;
//            lock (progress_Handle)
//            {
//                tmp = m_progress;
//            }
//            return tmp;
//        }
//        set
//        {
//            lock (progress_Handle)
//            {
//                m_progress = value;
//            }
//        }
//    }
//    public ErrCode errorCode
//    {
//        get
//        {
//            ErrCode tmp;
//            lock (errorCode_Handle)
//            {
//                tmp = m_errorCode;
//            }
//            return tmp;
//        }
//        set
//        {
//            lock (errorCode_Handle)
//            {
//                m_errorCode = value;
//            }
//        }
//    }
//    public PhaseCode phaseCode
//    {
//        get
//        {
//            PhaseCode tmp;
//            lock (phaseCode_Handle)
//            {
//                tmp = m_phaseCode;
//            }
//            return tmp;
//        }
//        set
//        {
//            lock (phaseCode_Handle)
//            {
//                m_phaseCode = value;
//            }
//        }
//    }


//    protected override void ThreadFunction()
//    {
        
//    }
//}