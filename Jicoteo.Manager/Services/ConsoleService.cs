using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Jicoteo.Manager.Services;

public class ConsoleService
{
    public ConsoleService()
    {
        Log = new ObservableCollection<string>();
    }

    public ObservableCollection<string> Log { get; }
}