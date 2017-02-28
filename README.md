# Client and Server
University project to make a simple client and server that communicate with one another. The client has to request a person and the server responds with that persons location. The client can also add new people or change the location of an existing person. Both the server and the client had to be able to handle HTTP 1.0, HTTP 0.9 and HTTP 1.1 and whois style requests. It can also make requests to web servers and works fine over localhost.

##Basic Use:
Both programs take in arguments as their input by simply typing the argumentsafter the program exe file in a terminal like this:

`location.exe samuel`

where location.exe is the name of the client exe file.

The server has a database of names that have a location associated to that name.You can query the location of a person by giving a name as an argument like this:

`location.exe sam`

You can also change the location a person is at by giving both a name and a location like this:

`location.exe sam "in a meeting"`

This will also add a person and a location to the database if they do not already exist. There are also some other arguments that the client will accept.

##Client Arguments:
###/h
/h is used to specify the host to connect to. For example:

`location.exe /h localhost` - sets the host to "localhost"

`location.exe /h google.com` - sets the host to "google.com"

IP addresses also work.

###/p 
/p is used to specify the port you want to use. For example:

`location.exe /p 43` - sets the port to 43.

###Protocol Types
/h0 is used to send HTTP 1.0 style requests.
/h9 is used to send HTTP 0.9 style requests.
/h1 is used to send HTTP 1.1 style requests.
If none of these are specified the default protocol is  a simplified whois.

These arguments can all be used together in any order. The name however must come before the location if a location is being changed.
Examples:

`location.exe /h localhost /p 43 samuel /h1`

`location.exe /h9 /p 80 samuel /h google.com`

`location.exe samuel /h 127.0.0.1 "is in a meeting" /p 13000 /h0`

`location.exe /h localhost /p 12000 samuel "in room 403"`

##Server Arguments:
###/d
/d enables debug mode which prints out additional useful information to the console if enabled. It shows you the response and request from the server as well as the reqest type. It is used as follows:

`locationserver.exe /d`

where 'locationserver.exe' is the name of the server executable.

###/l
/l enables the server log. This will log all server activities to a txt file in common log format. It is used as follows:

`locationserver.exe /l C:\Users\username\Desktop\log.txt`

/l must be followed by a file path for the log file to be saved to seperated by a single space. However, a file does not have to be specified as one will be created called 'log.txt' if one is not found in the given directory. For example:

`locationserver.exe /l C:\Users\username\Desktop\`

###/f
/f enables saving of the database to a txt file. It is used in a similar way to /l above.

`locationserver.exe /f C:\Users\username\Desktop\database.txt`

This would tell the server to save the database to the file at the given directory while it is in use. It is also used for loading a database as well. The file at the given  directory would be loaded in and then saved when changed to the database are made. Just like /l a file does not need to be specified and will be created in the given directory.

Again all these can be used together in any order as long as /l and /f have a file path after them. For example:

`locationserver.exe /d /l C:\Users\username\Desktop\log.txt /f C:\Users\username\Desktop\database.txt`

`locationserver.exe /l C:\Users\username\Desktop\log.txt /d`

`locationserver.exe /d C:\Users\username\Desktop\ /d /l C:\Users\username\Desktop\`

