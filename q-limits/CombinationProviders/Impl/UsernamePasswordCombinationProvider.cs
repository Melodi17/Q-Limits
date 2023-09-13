using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace q_limits.CombinationProviders.Impl
{
    public class UsernamePasswordCombinationProvider : ICombinationProvider<UsernamePasswordCombination>
    {
        private IEnumerable<string> _usernames;
        private IEnumerable<string> _passwords;

        private string _username;
        private string _password;

        public string[] GetFactorialNames() => new[] { "username", "password" };
        
        public UsernamePasswordCombinationProvider()
        {
            this._usernames = null;
            this._username = null;
            this._passwords = null;
            this._password = null;
        }

        public void SetLogin(string username) => this._username = username; 

        public void SetLoginCollection(IEnumerable<string> usernames) => this._usernames = usernames;

        public void SetPassword(string password) => this._password = password;

        public void SetPasswordCollection(IEnumerable<string> passwords) => this._passwords = passwords;
        public int GetCombinationCount() => (this._usernames?.Count() ?? 1) * (this._passwords?.Count() ?? 1);

        IEnumerable<ICombination> ICombinationProvider.EnumerateCombinations() => this.EnumerateCombinations();
        public IEnumerable<UsernamePasswordCombination> EnumerateCombinations()
        {
            foreach (string username in this._usernames ?? new[] { this._username })
                foreach (string password in this._passwords ?? new[] { this._password })
                    yield return new(username, password);
        }
    }
    
    public record UsernamePasswordCombination(string Username, string Password) : ICombination
    {
        public Dictionary<string, string> GetFields() => new()
        {
            { "username", this.Username },
            { "password", this.Password }
        };
    }
}
