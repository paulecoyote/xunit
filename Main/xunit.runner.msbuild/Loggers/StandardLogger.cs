using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Diagnostics;

namespace Xunit.Runner.MSBuild
{
    public class StandardLogger : IRunnerLogger
    {
        protected readonly TaskLoggingHelper log;
        public int Total = 0;
        public int Failed = 0;
        public int Skipped = 0;
        public double Time = 0.0;

        public StandardLogger(TaskLoggingHelper log)
        {
            this.log = log;
        }

        public void AssemblyFinished(string assemblyFilename, int total, int failed, int skipped, double time)
        {
            log.LogMessage(MessageImportance.High,
                           "  Tests: {0}, Failures: {1}, Skipped: {2}, Time: {3} seconds",
                           total,
                           failed,
                           skipped,
                           time.ToString("0.000"));

            Total += total;
            Failed += failed;
            Skipped += skipped;
            Time += time;
        }

        public void AssemblyStart(string assemblyFilename, string configFilename, string xUnitVersion)
        {
        }

        public bool ClassFailed(string className, string exceptionType, string message, string stackTrace)
        {
            string file;
            int line;

            GetFilenameAndLineFromStacktrace(stackTrace, out file, out line);
            log.LogError(null, null, null, file, line, null, null, null, "[CLASS] {0}: {1}", className, Escape(message));
            return true;
        }

        public void ExceptionThrown(string assemblyFilename, Exception exception)
        {
            StackTrace stackTrace = new StackTrace(exception, true);
            StackFrame frame = stackTrace.GetFrame(0);
            string file = frame.GetFileName();
            int line = frame.GetFileLineNumber();
            int col = frame.GetFileColumnNumber();
            log.LogError(null, null, null, file, line, col, 0, 0, exception.Message + Environment.NewLine + string.Format("While running: {0}", assemblyFilename));
        }

        public void TestFailed(string name, string type, string method, double duration, string output, string exceptionType, string message, string stackTrace)
        {
            string file;
            int line;
            GetFilenameAndLineFromStacktrace(stackTrace, out file, out line);
            log.LogError(null, null, null, file, line, 0, 0, 0, "{0}: {1}", name, Escape(message));
            WriteOutput(output);
        }

        public bool TestFinished(string name, string type, string method)
        {
            return true;
        }

        public virtual void TestPassed(string name, string type, string method, double duration, string output)
        {
            log.LogMessage("    {0}", name);
            WriteOutput(output);
        }

        public void TestSkipped(string name, string type, string method, string reason)
        {
            log.LogWarning(null, null, null, "Skipping test", 0, 0, 0, 0, "{0}: {1}", name, Escape(reason));
        }

        public virtual bool TestStart(string name, string type, string method)
        {
            return true;
        }

        static string Escape(string value)
        {
            return value.Replace(Environment.NewLine, "\n");
        }

        static void GetFilenameAndLineFromStacktrace(string stackTrace, out string file, out int line)
        {
            // "at <whatever> in <file>:line <line>[\n]"
            var newlineIndex = stackTrace.IndexOf('\n');
            if (newlineIndex > 0)
            {
                stackTrace = stackTrace.Remove(newlineIndex);
            }

            file = stackTrace.Substring(stackTrace.IndexOf("in ") + 3);
            var colonIndex = file.LastIndexOf(':');
            if (colonIndex > 0)
            {
                file = file.Remove(colonIndex);
                line = int.Parse(stackTrace.Substring(stackTrace.LastIndexOf(" ")));
            }
            else
            {
                line = 0;
            }
        }

        protected void WriteOutput(string output)
        {
            if (output != null)
            {
                log.LogMessage("    Captured output:");
                foreach (string line in output.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    log.LogMessage("      {0}", line);
            }
        }
    }
}