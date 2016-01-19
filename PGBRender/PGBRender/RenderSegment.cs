﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace PGBRender
{
    class RenderSegment
    {
        public ProcessComplete OnComplete;
        public ProcessState State { get { return _State; } }
        private ProcessState _State { get; set; }
        public string BlendFile { get; set; }
        public string OutputDirectory { get; set; }
        public int StartFrame { get; set; }
        public int EndFrame { get; set; }
        public string OutputPath
        {
            get
            {
                string outputName = Path.GetFileNameWithoutExtension(BlendFile) + " Render";
                string outputFull = Path.Combine(OutputDirectory, outputName);
                return outputFull + " " + StartFrame + "-" + EndFrame;
            }
        }

        public RenderSegment()
        {
            _State = ProcessState.Prestart;
        }

        public void Render()
        {
            ThreadStart start = new ThreadStart(RenderInternal);
            Thread sprinter = new Thread(start);
            sprinter.Start();
        }

        private void RenderInternal()
        {
            Process blenderRenderer = new Process();
            ProcessStartInfo blenderArgs = new ProcessStartInfo(@"C:\Program Files\Blender Foundation\Blender\blender.exe", BuildCommandLineArgs());
            blenderArgs.CreateNoWindow = true;
            blenderArgs.RedirectStandardOutput = true;
            blenderArgs.UseShellExecute = false;
            blenderRenderer.StartInfo = blenderArgs;
            blenderRenderer.Start();
            _State = ProcessState.Running;
            string output = blenderRenderer.StandardOutput.ReadToEnd();
            _State = ProcessState.Complete;

            if (OnComplete != null)
                OnComplete();
        }

        private string BuildCommandLineArgs()
        {
            string outputName = Path.GetFileNameWithoutExtension(BlendFile) + " Render #";
            string outputFull = Path.Combine(OutputDirectory, outputName);
            return string.Format(@"-b ""{0}"" -E BLENDER_RENDER -s {1} -e {2} -o ""{3}"" -a", BlendFile, StartFrame, EndFrame, outputFull);
        }
    }

    delegate void ProcessComplete();

    enum ProcessState
    {
        Prestart,
        Running,
        Complete
    }
}
