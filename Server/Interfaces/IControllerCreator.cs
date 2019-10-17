using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Interfaces
{
    /// <summary>
    /// Инерфейс фабрики по созданию контроллеров по имени
    /// </summary>
    public interface IControllerCreator
    {
        Controller Create(string name);
    }
}
