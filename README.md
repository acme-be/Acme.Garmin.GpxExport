# Acme.Garmin.GpxExport
Small .Net Core tool to download all activities and gpx in Garmin.

## Usage
- Get the sources and compile the application
- Connect via your browser into https://connect.garmin.com/modern/
- Get the value of the cookie "SESSIONID" in your browser
- Launch the application
- Enter the sessionId
- Wait ... :)

## Known issues
- Sometimes the download return a (403) Forbidden for the sessionId, the application try 5 times, because it seems to be a temporary error :'(
