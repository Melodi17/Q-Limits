using System.Collections;
using System.Collections.Generic;

namespace q_limits.CombinationProviders
{
    public interface ICombination
    {
        Dictionary<string, string> GetFields();
    }
    public interface ICombinationProvider
    {
        string[] GetFactorialNames();
        void SetLogin(string username);
        void SetLoginCollection(IEnumerable<string> usernames);
        void SetPassword(string password);
        void SetPasswordCollection(IEnumerable<string> passwords);
        int GetCombinationCount();
        IEnumerable<ICombination> EnumerateCombinations();
    }

    public interface ICombinationProvider<T> : ICombinationProvider where T : ICombination
    {
        new IEnumerable<T> EnumerateCombinations();
    }
}