using UnityEngine;
public abstract class Mode
{
    public abstract void Work();
}

public class UserMode : Mode
{
    public override void Work()
    {
        Debug.Log("User");
        return;
    }
}

public class ManagerMode : Mode
{
    public override void Work()
    {
        Debug.Log("Manager");
        return;
    }
}

public class Context
{
    private Mode _mode;

    public Context()
    {
        _mode = new UserMode();
    }

    /// <summary>
    /// Switch to User mode.
    /// </summary>
    public void SetUserMode()
    {
        _mode = new UserMode();
    }

    /// <summary>
    /// Switch to Manager mode.
    /// </summary>
    public void SetManagerMode()
    {
        _mode = new ManagerMode();
    }

    /// <summary>
    /// Do the work of this mode.
    /// </summary>
    public void Run()
    {
        _mode.Work();
    }
}