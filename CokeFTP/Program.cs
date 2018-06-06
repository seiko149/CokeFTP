using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Renci.SshNet;

namespace CokeFTP
{
    class Program
    {

        public static String GetTimestamp()
        {
            DateTime value = DateTime.Now;
            return value.ToString("yyyyMMdd");
        }

        public static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------------------");
        }

        static void Main(string[] args)
        {
            string drive = @"C:\CokeFTP";
            string fileName = @"\SALES.txt";
            string fullFile = drive + fileName;
            string logDir = @"C:\CokeFTP\Log";
            string logFile = @"C:\CokeFTP\Log\log.txt";
            string timeStamp = GetTimestamp();
            string archiveDirectory = @"C:\CokeFTP\archive";
            string username = "coke";
            string password = "test";

            try
            {
                if (!Directory.Exists(drive))
                    Directory.CreateDirectory(drive);

                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                if (!File.Exists(logFile))
                    File.Create(logFile);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error creating logging directory!");
                Console.WriteLine(ex.ToString());
            }

            File.GetAccessControl(logFile);

            StreamWriter w = File.AppendText(logFile);
            

            try
            {
                Log("Starting...", w);
                Log("Checking for local file...", w);
                // Check to see if file exists.  If so, exit.

                if (!File.Exists(fullFile))
                {
                    Log("Local file does not exist!", w);
                    w.Close();
                    Environment.Exit(0);
                }

                // Rename file
                string timeStampFile = drive + @"\SALES_" + timeStamp + ".txt";
                File.Move(fullFile, timeStampFile);

                // Open connection!
                Log("Connecting to SFTP site...", w);
                var sftp = new SftpClient("data.shoppertrak.com", 22, username, password);
                sftp.Connect();

                FileStream fileStream = new FileStream(timeStampFile, FileMode.Open);
                // Upload file
                Log("Uploading file...", w);
                sftp.UploadFile(fileStream, @"SALES_" + timeStamp + ".txt");
                fileStream.Close();
                // Archive File
                Log("Archiving file.  Creating archive directory if necessary...", w);
                if (!Directory.Exists(archiveDirectory))
                    Directory.CreateDirectory(archiveDirectory);

                string archiveName = archiveDirectory + @"\SALES_" + timeStamp + ".txt";

                Log("Moving file to archive...", w);
                System.IO.File.Move(timeStampFile, archiveName);
                Log("Success!  Disconnecting...", w);
                sftp.Disconnect();
                w.Close();
            }

            catch (Exception ex)
            {
                Log("****** ERROR !", w);
                Log(ex.ToString(), w);
            }
        }
    }
}