namespace rvfun.lib;

public class Logger
{
    public Logger()
    {
        this.Errors = new List<string>();
    }

    public List<string> Errors {get;}

    public void ReportError(string errorMsg)
    {
        this.Errors.Add(errorMsg);
        Console.Out.WriteLine(errorMsg);
    }

}