using System.Collections.ObjectModel;

namespace LCSC.Manager.Services;

public class ConsoleService
{
    public ConsoleService()
    {
        Log = new ObservableCollection<string>();
    }

    public ObservableCollection<string> Log { get; }
}