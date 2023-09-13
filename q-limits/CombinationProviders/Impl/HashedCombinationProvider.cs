using System;
using System.Collections.Generic;
using System.Linq;

namespace q_limits.CombinationProviders.Impl
{
    public class HashedCombinationProvider : ICombinationProvider<HashedCombination>
    {
        private IEnumerable<string> _passwords;
        private IEnumerable<string> _hashes;
        
        private string _password;
        private string _hash;
        
        private Func<string, string> _hashFunc;

        public string[] GetFactorialNames() => new[] { "password", "posthash", "hash" };
        
        public HashedCombinationProvider(Func<string, string> hashFunc)
        {
            this._passwords = null;
            this._password = null;
            this._hashes = null;
            this._hash = null;
            this._hashFunc = hashFunc;
        }
        public void SetLogin(string username) => throw new("Cannot supply login factorial to this provider");
        public void SetLoginCollection(IEnumerable<string> usernames) => throw new("Cannot supply login (collection) factorial to this provider");
        public void SetPassword(string password) => this._password = password;
        public void SetPasswordCollection(IEnumerable<string> passwords) => this._passwords = passwords;
        
        public void SetHash(string hash) => this._hash = hash;
        public void SetHashCollection(IEnumerable<string> hashes) => this._hashes = hashes;
        public void RemoveHash(string hash)
        {
            lock (this._hashes)
                this._hashes = this._hashes.Where(x => !x.Equals(hash, StringComparison.OrdinalIgnoreCase));
        }


        public int GetCombinationCount() => (this._passwords?.Count() ?? 1) * (this._hashes?.Count() ?? 1);

        IEnumerable<ICombination> ICombinationProvider.EnumerateCombinations() => this.EnumerateCombinations();
        public IEnumerable<HashedCombination> EnumerateCombinations()
        {
            foreach (string password in this._passwords ?? new[] { this._password })
            {
                string postHash = this._hashFunc(password);
                lock (this._hashes)
                    foreach (string hash in this._hashes ?? new[] { this._hash })
                        yield return new(password, postHash, hash);
            }
        }
    }

    public record HashedCombination(string Password, string PostHash, string Hash) : ICombination
    {
        public Dictionary<string, string> GetFields() => new()
        {
            { "password", this.Password },
            { "posthash", this.PostHash },
            { "hash", this.Hash }
        };
    }
}
