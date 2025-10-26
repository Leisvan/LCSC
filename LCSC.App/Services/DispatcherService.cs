using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCSC.App.Services;

public class DispatcherService
{
    private readonly DispatcherTimer _timer;

    public DispatcherService()
    {
        _timer = new DispatcherTimer();
    }
}