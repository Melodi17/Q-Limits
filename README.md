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

#### Examples



### Change Log

- **[Version 2.0.4]** Progress bar spinner now is blue
- **[Version 2.0.4]** Implemented write output instead of throwing an error when the specified module is not found
- **[Version 2.0.4]** Shows error when insufficient arguments are supplied
- **[Version 2.0.3]** Added max_thread_count parameter
- **[Version 2.0.2]** Added HTTP Form and FTP protocols
- **[Version 2.0.2]** Cancel Key Press shows statistics before exiting

### To Do

- [ ] **[Top Priority]** Implement a proper command line parser (https://github.com/commandlineparser/commandline), in order to make the code more reliable and 'clean'. Fixing: https://github.com/Melodi17/Q-Limits/issues/2
- [ ] Get password generation parameter (-x) working
- [x] Write output instead of throwing an error when the specified module is not found
- [ ] Move the statistics bar appear after progress bar when the application receives Cancel Key Press
- [ ] Write help menu
- [ ] Write help text for each module
- [ ] Add POP3 protocol
- [ ] Add Telnet protocol
- [ ] Finish usage part of README.md