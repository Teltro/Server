using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Interfaces
{
    public interface IControllerCreator
    {
        Controller Create(string name);
    }
}
