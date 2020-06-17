using UnityEngine;
using UnityEngine.Events;
public abstract class Mode
{
    public UnityAction _work;
    public abstract void Work();
    public Mode(UnityAction work)
    {
        _work = work;
    }
}

public class UserMode : Mode
{
    public UserMode(UnityAction work) : base(work) { }

    public override void Work()
    {
        _work.Invoke();
        return;
    }
}

public class ManagerMode : Mode
{
    public ManagerMode(UnityAction work) : base(work) { }
    public override void Work()
    {
        _work.Invoke();
        return;
    }
}

public class Context
{
    private Mode _mode;
    UnityAction _managerWork;
    UnityAction _userWork;

    public Context(UnityAction userWork, UnityAction managerWork)
    {
        _userWork = userWork;
        _managerWork = managerWork;
        _mode = new UserMode(_userWork);
    }

    /// <summary>
    /// Switch to User mode.
    /// </summary>
    public void SetUserMode()
    {
        _mode = new UserMode(_userWork);
    }

    /// <summary>
    /// Switch to Manager mode.
    /// </summary>
    public void SetManagerMode()
    {
        _mode = new ManagerMode(_managerWork);
    }

    /// <summary>
    /// Do the work of this mode.
    /// </summary>
    public void Run()
    {
        _mode.Work();
    }
}