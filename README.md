# uptime-kuma-pushr

The pushr app is desinged to work with the wonderful https://github.com/louislam/uptime-kuma app. It works by pushing messages to a "push" monitor on uptime kuma and can be seen as a relay or external monitor.

To add a new monitor, open the app and use the 'A' option to add a new Monitor. Now you have to select what type of monitor you want to use
```
Please enter the Monitor type you want to add

         1  - Directory Exists
         2  - File Exists
         3  - File Exists
         4  - Named Processes
         5  - System Uptime
Select Monitor Type:
```
Select your disired monitor and continue. In the next step you must first setup some basic properties that all monitors have. Thoese include:
- The Name of the Monitor (can be different from the Kuma name)
- The Push interval (should be the same or less then the Kuma configured one)
- The Push URL (must be exactly the same as in Kuma)

To get the Push URL, open your Kuma instance and add a new Monitor there. When setting the type use "Push"
![image](https://user-images.githubusercontent.com/6794763/197037485-cc065dde-8d03-4b94-b42d-5125b9bb10a2.png)

and you will get a "Push Url" that looks like this: "https://DOMAIN/api/push/CODE?status=up&msg=OK&ping=". Take the whole url and enter it into pushr.

After the basic properties there might be some other settings you have to enter for the specific monitor.

When done, Your Monitor is saved and immeditally started.
