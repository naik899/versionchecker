using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersionChecker.Models
{
    public class AssemblyStructureModel
    {
        public string Name;
        public Dictionary<string, string> References;
        public int Count;
    }
}
