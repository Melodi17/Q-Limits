using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace q_limits.Modules
{
    public abstract class IModule
    {
        public string Name;
        public string ID;

        public abstract void Load(CommandLineOptions options, CredentialContext credContext, ProgressContext progCtx);
    }
}
