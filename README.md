# BuildServer_ProcessPool
Mother Build Server on command, spawns pool of processes of Child builders and builds libraries for the files specified by the client through GUI in the Repository server and delivers the libraries to Test Harness server.
While building the libraries, if a process dies, it won't affect the other processes running. Overcame the problems with threads, if a thread fails it brings down the whole process.
Used WCF for Message passing Communication service among the Federation servers.
•	Used WPF for developing Graphical User Interface for the client.
