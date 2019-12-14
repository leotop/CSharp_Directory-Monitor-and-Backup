using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Diagnostics;

namespace k163810_Q5
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            //Files_Backup filebackup = new Files_Backup();
            //filebackup.OnDebug();
            Change_Notifier cn = new Change_Notifier();
            cn.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Files_Backup(),
                new Change_Notifier()
                
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
