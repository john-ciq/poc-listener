using System;
using System.Windows;
using System.Diagnostics;
using ChartIQ.Finsemble;
using System.IO;

namespace AppListener
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		public static string[] args = null;
        private MainWindow mainWindow = null;

		public App()
        {
			Trace.TraceInformation("App started");
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			args = e.Args;
//#if DEBUG
//			Debugger.Launch();
//#endif

//#if LOGGING && TRACE
			TextWriterTraceListener logger = new TextWriterTraceListener("Finsemble.log");
			logger.TraceOutputOptions = TraceOptions.DateTime;

			Trace.Listeners.Add(logger);
			Trace.AutoFlush = true;
			Trace.TraceInformation("Logging started");
//#endif
		}

		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			LogUnhandledException(e.Exception);
			Finsemble.DispatcherUnhandledException(mainWindow, e);

			Trace.TraceError($"An Unhandled Exception has occurred. Exception: {e.Exception}");
			Trace.TraceInformation("Shutting down");
			Shutdown();
		}

		private void LogUnhandledException(Exception e)
		{
			using (StreamWriter sw = new StreamWriter("Critical exceptions.log", true))
			{
				sw.WriteLine($"{DateTime.Now.ToUniversalTime()} - {e.Message}");
				sw.WriteLine(e.StackTrace);
				sw.WriteLine();
				sw.Close();
			}

			if (e.InnerException != null) LogUnhandledException(e.InnerException);
		}

	}

}
