﻿using System;
using System.Threading;
using System.Windows.Forms;
using SyncClipboard.Control;

namespace SyncClipboard
{
    public class PullService
    {
        Notify Notify;
        public bool switchOn = false;

        public PullService(Notify notifyFunction)
        {
            Notify = notifyFunction;
            Load();
        }

        public void Load()
        {
            if (Config.IfPull)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        public void Start()
        {
            if (!switchOn)
            {
                Thread pullThread = new Thread(PullLoop);
                pullThread.SetApartmentState(ApartmentState.STA);
                pullThread.Start();
                switchOn = true;
            }
        }

        public void Stop()
        {
            switchOn = false;
        }

        private void PullLoop()
        {
            string oldString = "";
            int errorTimes = 0;
            while (switchOn)
            {
                String strReply = "";
                try
                {
                    Console.WriteLine("pull start " + DateTime.Now.ToString());
                    strReply = HttpWebResponseUtility.GetText(Config.GetProfileUrl(), Config.TimeOut, Config.GetHttpAuthHeader());
                    errorTimes = 0;
                    Console.WriteLine("pull end " + DateTime.Now.ToString());
                }
                catch (Exception ex)
                {
                    errorTimes += 1;
                    Console.WriteLine(ex.ToString());
                    Notify(false, true, ex.Message.ToString(), Config.GetProfileUrl() + "\n重试次数:" + errorTimes.ToString(), "重试次数:" + errorTimes.ToString(), "erro");
                    continue;
                }

                errorTimes = 0;
                Profile profile = new Profile(strReply);
                if (profile.Text != oldString)
                {
                    Clipboard.SetData(DataFormats.Text, profile.Text);
                    oldString = profile.Text;
                    Notify(true, false, "剪切板同步成功", oldString, null, "info");
                }
                Notify(false, true, "服务器连接成功", null, "正在同步", "info");
                
                Thread.Sleep((int)Config.IntervalTime);
            }
        }
    }
}