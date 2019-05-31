using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Stickey
{
    public static class GenericLogging
    {
        private static readonly TelemetryClient Client = new TelemetryClient();

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Client.TrackException(ex, new Dictionary<string, string>
            {
                {"isTerminating", e.IsTerminating.ToString() },
            });
            Client.Flush();

            // wait for client to finish flushing
            if (e.IsTerminating) Thread.Sleep(2000);
        }
    }
}
