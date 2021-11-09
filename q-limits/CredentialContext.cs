using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace q_limits
{
    public class CredentialContext
    {
        public bool SupportUsernames => Usernames.Any();
        public string[] Usernames;

        public bool SupportPasswords => Passwords.Any();
        public string[] Passwords;

        public int Combinations => Usernames.Length + Passwords.Length;
    }
}
