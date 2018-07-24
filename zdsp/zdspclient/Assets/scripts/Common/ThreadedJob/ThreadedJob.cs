using System;
using System.Collections;

public class ThreadedJob
{
    private bool m_IsDone = false;
    private object m_Handle = new object();
    private System.Threading.Thread m_Thread = null;

    public bool IsDone
    {
        get
        {
            bool tmp;
            lock (m_Handle)
            {
                tmp = m_IsDone;
            }
            return tmp;
        }
        set
        {
            lock (m_Handle)
            {
                m_IsDone = value;
            }
        }
    }

    Action errorHandler;
    public virtual void Start(Action<Exception> handler = null)
    {
        Exception exception = null;
        m_Thread = new System.Threading.Thread(() => Run(out exception));
        m_Thread.Start();
        errorHandler = () => { if(handler != null) handler(exception); };
    }

    public virtual void Abort()
    {
        m_Thread.Abort();
    }

    protected virtual void ThreadFunction() { }

    protected virtual void OnFinished()
    {
        m_Thread.Join();
        errorHandler.Invoke();
    }

    public virtual bool Update()
    {
        if (IsDone)
        {
            OnFinished();
            return true;
        }
        return false;
    }

    public IEnumerator WaitFor()
    {
        while (!Update())
        {
            yield return null;
        }
    }

    private void Run(out Exception exception)
    {
        exception = null;
        try
        {
            ThreadFunction();
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        IsDone = true;
    }
    
}
