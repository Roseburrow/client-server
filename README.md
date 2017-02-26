# Client and Server
Uni project to make a simple client and server that communicate with one another.

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
