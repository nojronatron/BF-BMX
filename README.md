# Bigfoot Bib Message eXtractor

The overarching goal of this project is to create a synchronization tool that will scrape Winlink Express messages for "Bigfoot Bib Data" and log that data to a central server computer.

## High Level Overview

- The "Bigfoot 200" by [Destination Trail](https://www.destinationtrailrun.com/) is a 200+ mile ultra trail marathon, located in the Gifford-Pinchot National Forest, in a 5-day event every summer. It is part of the "(in) famous triple crown" of 200-mile ultra marathon events.
- Ham radio operators from around the Pacific Northwest volunteer to support this event by providing logistical, tactical, and emergency communications alongside the event organizers and volunteers.
- Over time the hams have devised a digital messaging process using [Winlink Express](https://www.winlink.org), and programs like MS Excel and MS Access to report on runner locations from each Aid Station to Race Officials at the Finish Line.
- Managing the inflow of data at the Finish Line is a challenging task and a means to synchronize data collection from multiple Winlink Express instances is desireable, so that Finish Line hams can focus on reporting runner position data to race officials in a timely manner.

## Project Status

10-Apr-2024:

- Added functional 3rd monitor.
- Updated Monitor Status Message content and behavior.
- Block configuring Monitors with duplicate file paths (although parent and child paths are allowed).
- Added checks to fend against edge-case Monitor states.
- Updated BibRecord detection pattern and processing.
- Rearranged BibRecord logging format to be tab-delimited entries: `[Winlink ID] [DateTime] [WarningFlag] [Bib Number] [Bib Action] [Bib Time] [Day Of Month] [Location]`
- Fixed concurrency bugs in Monitor processing.

5-Apr-2024:

- Added UI feature that displays latest detected file paths.

4-Apr-2024:

- Improved logging to capture more data server-side.
- Updated Environment Variables requirements for both client and server.

3-Mar-2024:

- Implemented tests and squashed bugs.
- Includes an exploratory models for later use in returning data from library to calling code.
- Includes exploratory ConcurrentQueue for possible use in displaying files found progress to the UI.

28-Feb-2024:

- Implemented early version of the server services (services in following bullet points).
- Implements use of Environment Variables for logging configuration.
- Server service logs server operations including error events, data or service warnings, and informational notes.
- Server service attempts to load a backup file to allow quickly resuming operations after a server restart or other failover requirements.
- POST Route receives data payload from client.
- GET Route enables anytime server data backup to a local JSON file.
- Logging manager logs incoming data to an auditing file.
- Logging manager stores bib data in expected format to a tab-delimited plain text file.
- Updated existing unit tests.
- Added unit tests for server-side implementations.

20-Feb-2024:

- Refactored data models for relational database storage and streamlined data transfer between client and server.
- Updated unit tests for new data models.
- Refactored bib data Regex matching to enable strict matching of bib data, and loose matching of possible bib data.
- Refactored file processing for enhance code testability and readability.

06-Feb-2024:

- Project design drawings underway.
- GitHub project initialized.

## Planned V.1.0 Features and Components

Client App:

- Monitor up to three Winlink Express instances on a single PC.
- Log all "Bigfoot Bib Data" and note any possible data issues.
- Contact Server and send data to it on the LAN (wired or wifi).
- Minimal UI, minimal necessary configuration needed.
- Limited on-screen logging of discovered Winlink Express messages.

Server Service:

- Listen for data from Client App(s) and log all incoming requests and payloads.
- Log all "Bigfoot Bib Data" and note any possible data issues with an additional "flag" bit.

## Target Environment And Dependencies

- Windows 10 or 11.
- The latest stable .NET 6 Runtime.
- A fully-connected wired or wireless LAN.
- The latest version of Winlink Express (to ensure mime-type compatibility).

Operators must be able to:

- Run executables in a Windows environment.
- Navigate the Windows Filesystem to find logfile(s).
- View plain-text logfile(s) using Notepad or similar (suggest Notepad++ or MS Excel).
- Configure Windows Firewall to allow HTTP communications between client and server.
- Install, manage, and operate Winlink Express.

## Solution Design

At a very high level:

- A client-side application monitors _up to three_ Winlink Express messages folder for new items. When found, any "bib records" discovered are recorded to a logfile and sent to a configured server service on the local area network (LAN).
- A server-side application listens for configured clients to send it data in a JSON format. When data is received, the server logs the "bib data" to a file that can be imported into a database or spreadsheet for further analysis.

## How To Use

This section will be updated as the project enters the Beta Testing phase.

### Configure and Run

1. See [Target Environment And Dependencies](#target-environment-and-dependencies) for the minimum requirements.
1. See [Configure Local Environment Variables](#configure-local-environment-variables) for configuring the Desktop App and Server service.
1. Copy the compiled Desktop App folder and its files to a convenient location on the client computer.
1. Copy the compiled Server Service folder and its files to a convenient location on the server computer (or client if running both on one machine).
1. Start the Server Service by double-clicking the BFBMX-Server-API.exe file, and accepting the Windows Firewall prompt. Make not of the `log location` presented in the Server Service console output window (do not close the console).
1. Start the Desktop App by double-clicking the BFBMX-Desktop-App.exe file. Make not of the `log location` that is presented in the Desktop App.
1. More steps to follow.

### Use the Desktop App

This section will include information such as: Setting up paths to monitor, starting and stopping monitors, and reviewing log files.

### Use the Server App

This section will include information such as: Reviewing Console Logging, finding and using logging output files.

### Configure Local Environment Variables

BFBMX will create a folder for its logfiles in the Documents folder of USERPROFILE, for example:

C:\Users\username\Documents

Environment Variables that apply only to the Server Service:

- `BFBMX_BACKUP_FILE_NAME` = Filename to store server data entries as readable JSON. Recommend a name like: "BFBMX-LocalDb-Backup.txt"
- `BFBMX_SERVER_FOLDER_NAME` =  Name of the folder where the server will store various other log files. Recommend a name like: "BFBMX_Server_Logs"

Environment Variables that apply to BOTH Desktop App and Server Service:

- `BFBMX_FOLDER_NAME` = Name of the folder where the Desktop App will store its log files. Recommend a name like: "BFBMX_Desktop_Logs"
- `BFBMX_SERVERNAME` = Name or IP address of server hosting the BF-BMX API. The server name or IP _must_ be accessible as `localhost` if local, or by an IP Address if remote (hostname if DNS is available on the network).
- `BFBMX_SERVERPORT` = The port that the BF-BMX API host is available on. Default port is `5150`. Check with the server computer operator to confirm correct port number configuration.

How to Set Environment Variables so they survive logout/restart:

1. Click `Start` and then `Settings` (or `CTRL + X` and then select `Settings`).
2. Left Nav Bar: Click `System`.
3. Right Content Listing: Click `About` (at the bottom of the list).
4. Click `Advanced system settings` to bring up the `System Properties` window.
5. Click `Advanced` tab.
6. Click button `Environment Variables...` (near the bottom).
7. There are two sections: User variables, and System variables.
8. Under `System Variables` click button `New...` to bring up the New System Variable window.
9. Type the Environment Variable `name` in the space to left of the equals sign.
10. Excluding quotation marks, copy or type the Variable `value` to the right of the equals sign.
11. Click `OK`.
12. Repeat steps 8-11 until all environment variable names and values have been entered.
13. `Close` the Environment Variables window and the System Properties window.

The computer operator(s) can then start the BF-BMX Desktop application(s) and Server Service.

## Notes and Limitations

- There is no way to ensure that a human-forwarded message is not altered in some. This software will not detect that type of change, and will assume the forwarded message is a valid candidate for processing a direct P2P or RMS-relayed Winlink Message.
- This software is designed to work specifically with tab-delimimted data. In the future, other delimiters may become available and will be documented here.
- There are conditions under which this software may not detect a newly created file in a monitored folder. While the author has made every effort to minimize these events from happening, it is not outside the realm of possibility. It is up to the Desktop App operator and the Server Service operator to review log files to ensure data is being processed as expected.

## Timeline

- A beta version will be made available to the Bigfoot Hams Coordinator for testing and evaluation by May 1st, 2024.
- Official v.1.0 is scheduled to be published by end of July 2024, in time for the August 9th-13th event.

## References

- [file-sync-win](https://github.com/nojronatron/file-sync-win)
- [Bigfoot-Bib-Report-WL-Form](https://github.com/nojronatron/Bigfoot-Bib-Report-WL-Form)
