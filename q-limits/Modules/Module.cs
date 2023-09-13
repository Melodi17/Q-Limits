using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using q_limits.CombinationProviders;

namespace q_limits.Modules
{
    public abstract class Module
    {
        public string Name;
        public string ID;
        
        public Module(string name, string id)
        {
            this.Name = name;
            this.ID = id;
        }
        
        public abstract ICombinationProvider GetCombinationProvider();

        public abstract void Load(CommandLineOptions options, ProgressContext progCtx);
    }
}
