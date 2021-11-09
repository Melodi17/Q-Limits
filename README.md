# Q-Limits
### Overview
The perfect tool for breaking limits, this is a login-cracking utility based off Hydra.

### Supported Protocols

The part in square brackets is the ID used to reference it

- HTTP Basic Authentication [`http-get`]
- HTTP Form [`http-get-form`]
- HTTP Proxy [`http-proxy`]
- FTP [`ftp`]
- SSH [`ssh`]
- SHA256 Hash [`sha256-hash`]
- MD5 Hash [`md5-hash`]

### Usage

```shell
q-limits [-m protcol] [-d destination] [-l login] [-L login_file] [-p password] [-P password_file] [-t max_thread_count] [-n [proxy_username]:[proxy_password]] [-s success_criteria] [-f fail_criteria]
```

### Change Log

- **[Version 2.0.3]** Added max_thread_count parameter
- **[Version 2.0.2]** Added HTTP Form and FTP protocols
- **[Version 2.0.2]** Cancel Key Press shows statistics before exiting

### To Do

