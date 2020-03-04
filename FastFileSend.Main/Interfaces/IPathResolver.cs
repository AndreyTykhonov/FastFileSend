using System;
using System.Collections.Generic;
using System.Text;

namespace FastFileSend.Main.Interfaces
{
    /// <summary>
    /// Resolving app pathes. 
    /// </summary>
    public interface IPathResolver
    {
        string UsersConfig { get; }
        string AccountConfig { get; }
        string Downloads { get; }
    }
}
