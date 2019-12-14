using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.ServiceProcess;
using System.Timers;
using System.Windows.Forms;


namespace k163810_Q5
{
    public partial class Change_Notifier : ServiceBase
    {
        private System.Timers.Timer t;
        private DateTime last_check;
        private string Backup_Folder;
        private string LocationToSaveLastUpdateCheckTime;
        private SmtpClient SmtpServer;
        private MailAddress to;
        private MailAddress from;
        private string Sub;


        public Change_Notifier()
        {
            InitializeComponent();
            Backup_Folder = ConfigurationManager.AppSettings["Backup_FolderPartB"];
            LocationToSaveLastUpdateCheckTime = ConfigurationManager.AppSettings["LocationToSaveLastUpdateCheckTime"];
            if (File.Exists(LocationToSaveLastUpdateCheckTime))
            {
                last_check = DateTime.Parse(File.ReadAllText(LocationToSaveLastUpdateCheckTime));
            }


            SmtpServer = new SmtpClient(ConfigurationManager.AppSettings["SmtpServer"])
            {
                Port = int.Parse(ConfigurationManager.AppSettings["Port"]),
                EnableSsl = Boolean.Parse(ConfigurationManager.AppSettings["EnableSsl"]),
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["From"]
                                                                        , ConfigurationManager.AppSettings["Password"]),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            to = new MailAddress(ConfigurationManager.AppSettings["To"]);
            from = new MailAddress(ConfigurationManager.AppSettings["From"]);
            Sub = "ChangeNotifier: Checked For Changes at ";
        }

        protected override void OnStart(string[] args)
        {
            t = new System.Timers.Timer(15 * 60 * 1000); //15 * 60 * 1000
            t.Elapsed += new ElapsedEventHandler(Check4Changes);
            t.Start();
        }

        protected void Check4Changes(object sender, ElapsedEventArgs e)
        {
            string[] filePaths = Directory.GetFiles(Backup_Folder, "*.*", SearchOption.AllDirectories);
            List<string> filePathsUpdated =  new List<string>();
            foreach (string file in filePaths)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastAccessTime > last_check)
                {
                    Debug.WriteLine(fi.LastAccessTime + " " + last_check);
                    filePathsUpdated.Add(file);
                }
            }
            last_check = DateTime.Now;
            if (filePathsUpdated.Count >  0)
            {
                SendEmail(filePathsUpdated.ToArray());
            }
            
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected void SendEmail(string[] files)
        {
            string msg = "";
            int i = 1;
            foreach (string file in files)
            {
                msg += i + ")" + Path.GetFileName(file) + ":".PadRight(30,' ') + new FileInfo(file).Length/1024 + "KB";
                //msg += String.Format("{0, -50}  {1,-10}", i + ")" + Path.GetFileName(file) + ":", new FileInfo(file).Length/1024 + "KB");
                msg += "\n";
                i++;
            }
            
            MailMessage message = new MailMessage(from, to)
            {
                Subject = Sub + last_check,
                Body = msg
            };

            try
            {
                SmtpServer.Send(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error Sending Email");
            }
        }

        protected override void OnStop()
        {
            Debug.WriteLine("OnStop");
            File.WriteAllText(LocationToSaveLastUpdateCheckTime, last_check.ToString());
        }
    }
}
