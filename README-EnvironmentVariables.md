# Configure Local Environment Variables

BFBMX will create a folder for its logfiles in the Documents folder of USERPROFILE with either Server or Desktop appended to the name, like:

- Desktop: `C:\Users\{username}\Documents\BFBMX_Desktop_Logs`
- Server: `C:\Users\{username}\Documents\BFBMX_Server_Logs`

BFBMX will also assume the Desktop and Server are running on the same machine on port `5150` unless you set Enivornment Variables.

Environment Variables that apply only to the Desktop App:

- `BFBMX_DESKTOP_LOG_DIR_`: Name of the folder where the Desktop App will store its log files. Recommend a name like "BFBMX_Desktop_Logs".

Environment Variables that apply only to the Server Service:

- `BFBMX_SERVER_LOG_DIR`: Name of the folder where the server will store log files. Recommend a name like "BFBMX_Server_Logs".

Environment Variables that apply to BOTH Desktop App and Server Service:

- `BFBMX_SERVER_NAME`: Name or IP address of server hosting the BF-BMX API. Use the server IPv4 address or `localhost` unless you know for certain a reliable name resolution service is available on your network.
- `BFBMX_SERVER_PORT`: The port that the BF-BMX API host is listening to. The default port is `5150` but the Server Operator might select another so check with them to verify before continuing.

How to Set Environment Variables so they survive logout/restart:

1. Click `Start` and then `Settings` (or `CTRL + X` and then select `Settings`).
1. Left Nav Bar: Click `System`.
1. Right Content Listing: Click `About` (at the bottom of the list).
1. Click `Advanced system settings` to bring up the `System Properties` window.
1. Click `Advanced` tab.
1. Click button `Environment Variables...` (near the bottom).
1. There are two sections: User variables, and System variables.
1. Under `System Variables` click button `New...` to bring up the New System Variable window.
1. Type the Environment Variable `name` in the space to left of the equals sign.
1. Excluding quotation marks, copy or type the Variable `value` to the right of the equals sign.
1. Click `OK`.
1. Repeat steps 8-11 until all environment variable names and values have been entered.
1. `Close` the Environment Variables window and the System Properties window.

The computer operator(s) can then start the BF-BMX Desktop application(s) and Server Service.
