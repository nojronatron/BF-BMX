# Bigfoot Bib Message eXtractor

The overarching goal of this project is to create a synchronization tool that will scrape Winlink Express messages for "Bigfoot Bib Data" and log that data to a central server computer.

## High Level Overview

- The "Bigfoot 200" by [Destination Trail](https://www.destinationtrailrun.com/) is a 200+ mile ultra trail marathon, located in the Gifford-Pinchot National Forest, in a 5-day event every summer. It is part of the "(in) famous triple crown" of 200-mile ultra marathon events.
- Ham radio operators from around the Pacific Northwest volunteer to support this event by providing logistical, tactical, and emergency communications alongside the event organizers and volunteers.
- Over time the hams have devised a digital messaging process using [Winlink Express](https://www.winlink.org), and programs like MS Excel and MS Access to report on runner locations from each Aid Station to Race Officials at the Finish Line.
- Managing the inflow of data at the Finish Line is a challenging task and a means to synchronize data collection from multiple Winlink Express instances is desireable, so that Finish Line hams can focus on reporting runner position data to race officials in a timely manner.

## Project Status

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

- A client-side application built for Windows 10+ will monitor Winlink Express for new messages, recording data from any new message that is a "Bigfoot Bib Report" to a logfile, and sending the data to a configured server service on the local area network (LAN).
- A server-side application listens for configured clients to send it data in a specialized format. When it receives data that matches, it logs the "Bigfoot Bib Data" to a file, which can be used for querying and reporting on data.

## How To Use

This section will be written as the project enters the Beta Testing phase.

## Notes and Limitations

- Messages can be forwarded by intermediary stations. This is different than an RMS _relay_ where message content is not changed. When an intermediary operator clicks "Forward" the original message body is not changed but new headers are added: Date-Time, From, Message-ID, etc, and the Subject line is modified - prefixed with "FW: ". The Message ID that this program tracks is the one that is attached to the outermost message header, not any attachments or forwarded headers. This should make it easier to sift through Winlink Message IDs at the receiving station to find a message with possible data problems.

## Timeline

- A beta version will be made available to the Bigfoot Hams Coordinator for testing and evaluation by May 1st, 2024.
- Official v.1.0 is scheduled to be published by end of July 2024, in time for the August 9th-13th event.

## References

- [file-sync-win](https://github.com/nojronatron/file-sync-win)
- [Bigfoot-Bib-Report-WL-Form](https://github.com/nojronatron/Bigfoot-Bib-Report-WL-Form)
