using System;
using System.Collections.Generic;
using System.Text;

using Server;
using Server.Interfaces;
using Server_TestProject.Models;
namespace Server_TestProject.Controllers
{
    /// <summary>
    /// Фабрика по созданию контроллеров
    /// </summary>
    public class ControllerCreator : IControllerCreator
    {
        ServerContext db;
        public ControllerCreator(ServerContext context)
        {
            db = context;
        }

        public Controller Create(string name)
        {
            if (name == "servers")
                return new ServersController(db);
            else if (name == "statistic")
                return new StatisticController(db);
            else return null;
        }

    }
}
