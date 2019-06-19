using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using System.Timers;
using System.Threading.Tasks;

using Microsoft.Win32;
using System.IO;

using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Task_Queue
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        static int excecutionDuration;
        static int excecutionQuality;
        static int taskClaimCheckPeriod;

        protected override void OnStart(string[] args)
        {
            //Debugger.Launch();
            WriteLog("hello15");
                
            excecutionDuration = getExcecutionDuration();
            excecutionQuality = getExcutionQuality();
            taskClaimCheckPeriod = getInterval();

            WriteLog($"excecutionDuration={excecutionDuration}\nexcecutionQuality={excecutionQuality}\ntaskClaimCheckPeriod={taskClaimCheckPeriod}");

            System.Timers.Timer t = new System.Timers.Timer(taskClaimCheckPeriod);
            t.Elapsed += new ElapsedEventHandler(WorkingCycleThread1);
            t.AutoReset = true;
            t.Enabled = true;
            t.Start();
            System.Threading.Thread thread = new System.Threading.Thread(WorkingCycleThread2);
            thread.Start();

        }

        static int getInterval()
        {

            int time;
            using (RegistryKey v = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Task_Queue\Parameters", true))
            {

                time = (int)v.GetValue("Task_Claim_Check_Period");

            }
            WriteLog(time.ToString());
            return time;
        }

        static int getExcecutionDuration()
        {
            int time;
            using (RegistryKey v = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Task_Queue\Parameters", true))
            {

                time = (int)v.GetValue("Task_Excecution_Duration");

            }
            WriteLog(time.ToString());
            return time;
        }

        static int getExcutionQuality()
        {
            //System.Diagnostics.Debugger.Break();
            int time;
            using (RegistryKey v = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Task_Queue\Parameters", true))
            {

                time = (int)v.GetValue("Task_Excecution_Quality");

            }
            WriteLog(time.ToString());
            return time;
        }

        public static void WorkingCycleThread1(object source, ElapsedEventArgs e)
        {

            //string regex = "Task_[0-9]{4}";
            //System.Diagnostics.Debugger.Launch();
            string[] claimsNames = readClaims();
            for (int i = 0; i < claimsNames.Length; i++)
            {
                Regex regex = new Regex("Task_[0-9]{4}");
                Match M = regex.Match(claimsNames[i]);
                if (M.Success)
                {
                    writeTask(claimsNames[i]);
                    WriteLog($"Задача {claimsNames[i]} успішно прийнта в обробку");
                    deleteClaim(claimsNames[i]);
                    break;
                }
                else
                    WriteLog($"Помилка розміщення заявки {claimsNames[i]}.Некоректний синтаксис");
                deleteClaim(claimsNames[i]);
            }
        }

        public static void deleteClaim(string claimName)
        {
            using (RegistryKey v = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Task_Queue\Claims", true))
            {

                v.DeleteValue(claimName);

            }
        }

        public static void writeTask(string task)
        {

            using (RegistryKey v = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Task_Queue\Tasks", true))
            {

                v.SetValue(task, "[....................]-Queued");
                
            }

        }

        public static string[] readClaims()
        {
            string[] res;

            using (RegistryKey v = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Task_Queue\Claims", true))
            {

                res = v.GetValueNames();

            }

            return res;
        }

        public static void WorkingCycleThread2()
        {
            var names = getTasks();
            while (true)
            {
                DoIt(names[0]);
            }
        }

        public static string[] getTasks()
        {
            string[] res;
            using (RegistryKey v = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Task_Queue\Tasks", true))
            {
                res = v.GetValueNames();
            }
            return res;
        }

        public static void DoIt(string param)
        {
            StringBuilder stringBuilder = new StringBuilder("[....................]",22);
            using (RegistryKey v = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Task_Queue\Tasks", true))
            {
                v.SetValue(param, "[....................]-In progress 0 % completed");
                for (int i = 1; i <= 20; i++)
                {
                    stringBuilder[i] = 'I';
                    v.SetValue(param, stringBuilder);
                    Thread.Sleep(2000);
                }
                v.SetValue(param, "[....................]-In progress 0 % completed");
            }
        }

        public static void WriteLog(string str)
        {

            string path = @"C:\Windows\Logs\TaskQueue_18-11-2013.txt";
            using (StreamWriter sw = new StreamWriter(path,true))
            {
                sw.WriteLine($"--------------------{DateTime.Now}-------------------");
                sw.WriteLine(str);
            }
        }

        protected override void OnStop()
        {
        }
    }
}
