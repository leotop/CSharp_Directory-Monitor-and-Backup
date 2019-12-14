using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;
using System.IO;
using System.Configuration;

namespace k163810_Q5
{
    public partial class Files_Backup : ServiceBase
    {
        private Timer t;
        private DateTime last_check;
        private string Folder2Backup;
        private string Backup_Folder;
        private string LocationToSaveLastBackUpTime;

        public Files_Backup()
        {
            InitializeComponent();
            Folder2Backup = ConfigurationManager.AppSettings["Folder2Backup"];
            Backup_Folder = ConfigurationManager.AppSettings["Backup_Folder"];

            if (!(Directory.Exists(Folder2Backup)) || !(Directory.Exists(Backup_Folder)))
            {
                throw new DirectoryNotFoundException();
            }

            LocationToSaveLastBackUpTime = ConfigurationManager.AppSettings["LocationToSaveLastBackUpTime"];

            if (File.Exists(LocationToSaveLastBackUpTime))
            {
                last_check = DateTime.Parse(File.ReadAllText(LocationToSaveLastBackUpTime));
            }
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            t = new Timer(60*1000);
            t.Elapsed += new ElapsedEventHandler(backup);
            t.AutoReset = false;
            t.Start();
        }

        private void backup_files(string[] files)
        {
            foreach (string file in files)
            {
                FileInfo fileObject = new FileInfo(file);
                string Dirpath = fileObject.Directory.FullName;
                string target = Backup_Folder + Dirpath.Substring(Folder2Backup.Length);
                Directory.CreateDirectory(target);
                File.Copy(file, target + "\\" + Path.GetFileName(file), true);
            }
        }


        protected void backup(object sender, ElapsedEventArgs e)
        {
            Debug.WriteLine("backing Up at " + DateTime.Now.ToString());
            bool updated = false;
            string[] filePaths = Directory.GetFiles(Folder2Backup, "*.*", SearchOption.AllDirectories);
            List<string> filePaths2backup = new List<string>();

            if (last_check == default(DateTime))
            {
                last_check = DateTime.Now;
                backup_files(filePaths);
                updated = true;
                Debug.WriteLine("time is default");
            }
            else
            {
                Debug.WriteLine("time is not default");
                foreach ( string file in filePaths) {
                    FileInfo fi = new FileInfo(file);
                    if (fi.LastWriteTime > last_check)
                    {
                        Debug.WriteLine(fi.LastAccessTime + " " + last_check);
                        filePaths2backup.Add(file);
                    }
                }
                last_check = DateTime.Now;
                backup_files(filePaths2backup.ToArray());
                if (filePaths2backup.Count > 0)
                {
                    Debug.WriteLine("count is greater then 0");
                    updated = true;
                }
            }

            // Interval changing as per the files updated
            if (!updated)
            {
                Debug.WriteLine("Not Updated ");
                if (t.Interval < (60 * 60 * 1000))
                {
                    Debug.WriteLine("if condition of Not Updated ");
                    t.Interval = t.Interval + ( 2 * 60 * 1000);    //2*60*1000
                }
            }
            t.Start();
            Debug.WriteLine(t.Interval);
            Debug.WriteLine("BackedUp at " + last_check);
            Debug.WriteLine("");
        }

        protected override void OnStop()
        {
            Debug.WriteLine("OnStop");
            File.WriteAllText(LocationToSaveLastBackUpTime, last_check.ToString());
        }
    }
}
