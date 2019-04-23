using System;

using Server;
using Server_TestProject.Models;
using Server_TestProject.Controllers;
namespace Server_TestProject
{
    class Program
    {

        static void Main(string[] args)
        {
            Host server = new Host(new ControllerCreator(new ServerContext()));


            DbFilling.DbFill(new ServerContext());
            bool IsRunning = true;
            int key;
            while (IsRunning)
            {
                Dialog();
                key = Int32.Parse(Console.ReadLine());
                switch (key)
                {
                    case 1:
                        server.Start();
                        break;
                    case 2:
                        server.Stop();
                        break;
                    case 3:
                        IsRunning = false;
                        break;
                    default:
                        Dialog();
                        break;

                }
            }

        }

        public static void Dialog()
        {
            Console.WriteLine("1 - Start Server");
            Console.WriteLine("2 - Stop Server");
            Console.WriteLine("3 - Exit");
        }
    }
}
