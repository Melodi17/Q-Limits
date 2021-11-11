# Q-Limits
### Overview
The perfect tool for breaking limits, this is a login-cracking utility based off Hydra.

### Supported Protocols

The part in square brackets is the ID used to reference it

- HTTP(S) Basic Authentication [`http-get`]
- HTTP(S) Form [`http-get-form`]
- HTTP(S) Proxy [`http-proxy`]
- FTP [`ftp`]
- SSH [`ssh`]
- SHA256 Hash [`sha256-hash`]
- MD5 Hash [`md5-hash`]

### Usage

```shell
q-limits [-m module] [-d destination] [-l login] [-L login_file] [-p password] [-P password_file] [-t max_thread_count] [-n [proxy_username]:[proxy_password]] [-s success_criteria] [-f fail_criteria]
```
####  Parameter Help

**-m *module***

Module/protocol (-m) is used to contact specified destination and is required unless '-h' or '-H' parameters are given. The '*module*' parameter is a string and must be a known module's ID, list is shown above.

**-d *destination***

Destination/Target (-m) is used to set the device that it targets during execution and is required unless '-h' or '-H' parameters are given. The '*destination*' parameter is a string and is an IPAddress or Hostname.

**-l *login***

Login/Username (-l) is used to append an item to a list of usernames to try. The '*login*' parameter is a string.

**-L *login_file***

Login file/Username file (-L) is used to append the file content to the list of usernames to try. The '*login_file*' parameter is a string, and must be a valid file path.

**-p *password***

Password/Key (-p) is used to append an item to a list of passwords to try. The '*password*' parameter is a string.

**-P *password_file***

Password file/Key file (-P) is used to append the file content to the list of passwords to try. The '*password_file*' parameter is a string, and must be a valid file path.

**-t *max_thread_count***

Max thread count/Thread count (-t) is used to limit the amount of threads the program can use for breaking limits. When the parameter is not included the default value will be set to 100. The '*max_thread_count*' parameter is a integer.

**-n *proxy_username*:*proxy_password***

Proxy authentication (-n) is used to set a proxy that certain protocols require if you are on a network and are requiring proxy authentication. It is only required if proxy authentication is necessary. The '*proxy_username*' parameter is a string and should be the username credentials for the proxy. The '*proxy_password*' parameter is a string and should be the password credentials for the proxy.

**-s *success_criteria***

Success criteria (-s) is used to set what defines a success in some modules (currently just http-get-form). The '*success_criteria*' parameter is a string.

**-f *fail_criteria***

Fail criteria (-f) is used to set what defines a failure in some modules (currently just http-get-form). The '*fail_criteria*' parameter is a string.

#### Examples

The following command will test login: 'root' and password: 'password' on the website 'localhost:8080/login' using the 'http-get' protocol

```shell
q-limits -m http-get -d localhost:8080/login -l root -p password
```

This will attempt to crack the supplied SHA256 hashes in the file 'hashes.txt' with the values in the password list from the file 'englishWordlist.txt'

```shell
q-limits -m sha256-hash -d hashes.txt -P englishWordlist.txt
```

This will attempt to solve login form at 'localhost:8080/login' by running through possible combinations and substituting '{LOGIN}' for the current login and the '{PASSWORD}' for the current password and checking if the content does not contain the fail criteria (-f) 'Incorrect', while using the specified proxy authentication.

```shell
q-limits -m http-get-form -d localhost:8080/login:user={LOGIN}&pass={PASSWORD} -l admin -P mostCommonPasswords.txt -f Incorrect -n myuser:mypass
```

### Change Log

- **[Version 2.0.5]** Finished usage part of README.md (-s success_criteria and -f fail criteria)
- **[Version 2.0.5]** Examples in README.md now has content
- **[Version 2.0.4]** Implemented a proper command line parser (https://github.com/commandlineparser/commandline), in order to make the code more reliable and 'clean'. Fixing: https://github.com/Melodi17/Q-Limits/issues/2
- **[Version 2.0.4]** Progress bar spinner now is blue
- **[Version 2.0.4]** Implemented write output instead of throwing an error when the specified module is not found
- **[Version 2.0.4]** Shows error when insufficient arguments are supplied
- **[Version 2.0.3]** Added max_thread_count parameter
- **[Version 2.0.2]** Added HTTP Form and FTP protocols
- **[Version 2.0.2]** Cancel Key Press shows statistics before exiting

### To Do

- [ ] Get password generation parameter (-x) working
- [ ] Add POP3 protocol
- [ ] Add http-post protocol
- [ ] Add http-form-post protocol