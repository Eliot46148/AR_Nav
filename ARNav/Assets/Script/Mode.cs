public abstract class Mode
{
    public abstract void work();
}

public class UserMode : Mode
{
    public override void work()
    {
        return;
    }
}

public class ManagerMode : Mode
{
    public override void work()
    {
        return;
    }
}

public class Context
{
    private Mode _mode;
}