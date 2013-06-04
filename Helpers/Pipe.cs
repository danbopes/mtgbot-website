using System;
using System.IO;
using System.IO.Pipes;

namespace MTGBotWebsite.Helpers
{
    public class Pipe
    {
        public static void SendMessage(string message, params object[] args)
        {
            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream("PipeToMtgBot"))
            {
                //The connect function will indefinately wait for the pipe to become available
                //If that is not acceptable specify a maximum waiting time (ms)
                try
                {
                    pipeStream.Connect(1000);

                    Console.WriteLine("[Client] Pipe Connection established");
                    using (var sw = new StreamWriter(pipeStream))
                    {
                        sw.AutoFlush = true;
                        sw.WriteLine(message, args);
                    }
                    pipeStream.WaitForPipeDrain();
                }
                catch (TimeoutException)
                {
                }
            }
        }
    }
}