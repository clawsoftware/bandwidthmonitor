# bandwidthmonitor in C#

Monitor the network bandwidth of all processes. Has the option to record the data only if the mobile connection is active. Uses windows event tracing.


# Screenshot


# Commandline

- /?                           Help
- /m=false|true                Log only when mobile connection is active. Default: false
- /csv=false|true              Log as CSV-File. Default: false
- /s=u|d                       Which Column to sort. Upload or download. Default: d
- /l="logfolder"               Logfile destination. Default: %EXEFOLDER%\bandwidthmonitor_%date%_%computer%_%randomnum%.log


# Changelog

## v1.0.0 (2019.01.19)

- initial version


# Dependencies
[Microsoft.Diagnostics.Tracing.TraceEvent](https://github.com/Microsoft/perfview)


# License
Copyright © 2019 Andrew Hess // clawSoft<br>
Licensed under MIT License
