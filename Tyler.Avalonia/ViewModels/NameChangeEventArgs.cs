using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tyler.ViewModels
{
    public struct NameChangeEventArgs
    {
        public string? OldName { get; }
        public string? NewName { get; }
        public object Entity { get; }

        public NameChangeEventArgs(Object e, string? oldName, string? newName)
        {
            Entity = e;
            OldName = oldName;
            NewName = newName;
        }
    }
}
