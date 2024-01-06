using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tyler.ViewModels
{
    public struct NameChangeEventArgs
    {
        public string OldName { get; }
        public string NewName { get; }

        public NameChangeEventArgs(string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }
    }
}
