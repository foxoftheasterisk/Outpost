using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace OutpostCore
{
    class Logger
    {

        //there may be better log solutions than this
        //but then, at least now if I find them it's easy to swap out.
        public static string logFile;

        private static string storedMessages = "";
        //since, sometimes we try to log multiple things at once...
        //(pretty sure there's a better way)

        public static void newLog(string filename)
        {
            logFile = filename;
            using (StreamWriter saveStream = new StreamWriter(logFile, true))
            {
                saveStream.WriteLine(DateTime.Now.ToLongTimeString() + ": Begin Log");
                saveStream.Flush();
                saveStream.Close();
            }
        }

        public static void Log(string message)
        {
            if (logFile == null)
                throw new InvalidOperationException("No log file specified!");

            try
            {
                using(StreamWriter saveStream = new StreamWriter(logFile, true))
                {
                    if(storedMessages != "")
                    {
                        saveStream.Write(storedMessages);
                        storedMessages = "";
                    }

                    saveStream.WriteLine(DateTime.Now.ToLongTimeString() + ": " + message);
                    saveStream.Flush();
                    saveStream.Close();
                }
            }
            catch(System.IO.IOException e)
            {
                storedMessages = storedMessages + DateTime.Now.ToLongTimeString() + ": " + message + "\n";
                //so... this actually comes up fairly often.
                //THERE MUST BE A BETTER WAY
                //TODO: find such.
            }
        }
    }
}
