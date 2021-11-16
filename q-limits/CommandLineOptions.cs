using CommandLine;

namespace q_limits
{
    public class CommandLineOptions
    {
        [Option('m', "module", Required = true, HelpText = "Module is used to contact specified destination")]
        public string Module { get; set; }

        [Option('d', "destination", Required = true, HelpText = "Destination is used to set the device that it targets during execution")]
        public string Destination { get; set; }
        
        [Option('l', "login", Required = false, HelpText = "Login is used to append an item to a list of usernames to try")]
        public string Login { get; set; }

        [Option('L', "login_file", Required = false, HelpText = "Login file is used to append the file content to the list of usernames to try")]
        public string LoginFile { get; set; }
        
        [Option('p', "password", Required = false, HelpText = "Password is used to append an item to a list of passwords to try")]
        public string Password { get; set; }
        
        [Option('P', "password_file", Required = false, HelpText = "Password file is used to append the file content to the list of passwords to try")]
        public string PasswordFile { get; set; }
        
        [Option('x', "password_generation", Required = false, HelpText = "Password generation is used to append the file content to the list of passwords to try")]
        public string PasswordGeneration { get; set; }

        [Option('X', "password_generation_extra_charset", Required = false, HelpText = "Password generation extra charsets are added to password generation is used to append the file content to the list of passwords to try")]
        public string PasswordGenerationXCharset { get; set; }
        
        [Option('t', "max_thread_count", Required = false, Default = 100, HelpText = "Max thread count is used to limit the amount of threads the program can use for breaking limits")]
        public int MaxThreadCount { get; set; }
        
        [Option('n', "proxy_authentication", Required = false, HelpText = "Proxy authentication is used to set a proxy that certain protocols require if you are on a network and are requiring proxy authentication")]
        public string Proxy { get; set; }
        
        [Option('s', "success_criteria", Required = false, HelpText = "")] // TODO: Write help text for this and put it in README.md
        public string SuccessCritera { get; set; }
        
        [Option('f', "fail_critera", Required = false, HelpText = "")] // TODO: Write help text for this and put it in README.md
        public string FailCritera { get; set; }
    }
}