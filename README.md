# Slime World MongoDB .NET Web API
This web application provides fast and easy access to slimeworlds, while keeping performance as fast as possible.
While the SlimeWorld plugin's MongoDb system opens connections to the database and has 14 pools by average open for every server, this web application keeps open only 1 connection open with 14 pools and stores SlimeWorlds in memory for faster access.

Saving is also optimized by having a list where when the save method gets called, worlds get stored in it and get saved every minute (@SlimeWorldTask.cs - set to 1 minute for testing, change it to your desire).
This doesn't always write to the database every save.
I recommend using this web application when having a lot of servers use slimeworld in it's gamemode (ex. SkyBlock Islands - Multiple synchronized SkyBlock servers, SkyWars map, Lobby map, etc.).
Considering that every server opens 14 pools for every connection, it can use a lot of RAM if not careful. MongoDB's one flaw is that a lot of connections lead to high RAM usage.

# How to setup
* Clone the project into a directory.
* Open up AppSettings (also AppSettings.Development for testing) and change configure it to your desire.
* Set the MongoDbConnectionString to your database's end point.
* Optional - Testing the project:
  * Start the project and use it's built-in **Swagger**.
  * Authenticate yourself by clicking on the **Authorize** button and inputting (without the <>): _ApiToken <your api token in appsettings>_
  * Click on one of the endpoint calls and call it by clicking **Try Out** -> **Execute**
  * If everything runs well, you should see a response of **200 OK**.
* Publishing:
  * Right click on your project in Visual Studio.
  * **Publish...**
  * Select the destination _(Folder, FTP, Docker, Azure...)_
  * Publish it.
  * If you selected Folder for publishing, copy the files into your server.
  * ⚠️ **DO NOT COPY IT INTO THE HOME DIRECTORY - THE FILES CAN BE ACCESSED FROM OUTER AND BE VIEWED - THIS EXPOSES APPSETTINGS WITH THE TOKEN**
* Running the project (Linux):
  * Install DOTNET SDK package:
    * wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    * sudo dpkg -i packages-microsoft-prod.deb
    * sudo apt-get update; \
      sudo apt-get install -y apt-transport-https && \
      sudo apt-get update && \
      sudo apt-get install -y dotnet-sdk-3.1
    * sudo apt-get update; \
      sudo apt-get install -y apt-transport-https && \
      sudo apt-get update && \
      sudo apt-get install -y aspnetcore-runtime-3.1
    * sudo apt-get install -y dotnet-runtime-3.1
  * Optional - Installing nginx:
    * Installing **nginx** package: apt-get install nginx
    * Running service: sudo service nginx start
    * Try to access nginx from your ip: http://<server_IP_address>/index.nginx-debian.html
      * If you cannot access it, then something went wrong.
    * Configure nginx - Edit /etc/nginx/sites-available/default
    * Replace all with next:
      ```
      server {
          listen        80;
          server_name   example.com *.example.com;
          location / {
              proxy_pass         http://localhost:5000;
              proxy_http_version 1.1;
              proxy_set_header   Upgrade $http_upgrade;
              proxy_set_header   Connection keep-alive;
              proxy_set_header   Host $host;
              proxy_cache_bypass $http_upgrade;
              proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
              proxy_set_header   X-Forwarded-Proto $scheme;
          }
      }
      ```
    * Replace server_name values with your server's domain (or IP if there's none).
    * Change the listen port to the port you want your API to be running on.
    * Restart service: sudo service nginx restart
    * If you have a firewall setup, then open the port.
    * Try to access the API from http://domain:port/getworldnames (or if the port is 80, then http://<domain>/getworldnames or http://<ip_address>/getworldnames)
    * If it returns **401 Unauthorized**, then it's running fine. If not, then look through the config again.

# Security
This project uses a custom API Token for authorizing which should be defined in the Headers of a request.
```
Authorization: ApiToken <your api token>
```

# How to use this on your server?
Well, at this time there is no custom loader written which is available for you, so it will be your task or someone else's to write one.
I have written one, but that will be available later in public. _Look out for that._

# Endpoints
## Get World By Name
```
Endpoint: /getWorldByName
Query parameters: worldName (string)
Method: GET
Usage: http://api.example.com/getWorldByName?worldName=exampleworld
Response format: 200 OK (json):
{
  "name": "exampleworld",
  "worldBytes": "sQsJBv//AAAAAgABAwAAAV0AAFsrKLUv/WArWp0KAKKNMTFwa6MA+Nb/////////jQbSM3tZO2Hko/zi1h2icbHY2MQSmhwgfMGSkYhbmkwiu6UP3weCIOg4Hg7lanfphmE1M6B3mxloT6NyF7p32ouBhX9NVq0FxQIKHoTHT78QDMPQ8ZyvtjGfIZdqnaUZ1UZER5qsbpj0JoZAwr/VpkKXmTajnLaZTDfTeoZ8RPA7Ks7/82MoComqZLQkxcG6PEyxYpBWvEhECIArxsoiLYmaYmTJFcrBT02S5ZrAyIbqihWTJR8NfTEgAAOKAKg0DgAU0PUvy+5U+PT4lPEk6WmEG7CK9+ta7DYVibqLqG6oLK5f2XztZpONobc07sgI2N5KlqohEU6CpkJcIw0gPRVwgQcwRFWG60JcG4JefuArvb5Bq2FzaeWnpv7wX1UAY8h7AO/Cx99/98H/FW7+r39SlqqI7loJirqs9FSVIip0AgAAABoAAAARKLUv/SARiQAACgAACQAFdGlsZXMKAAAAAAAAAAAAcgAAAIMotS/9IINNAwCiRhYbgLkZAEAKubeyfTsUe1QwAAsYZmB6CGMMWekHny0Lq4waAuOKzWUlhNxoy1gcRTkJDvhHJH8CNakY8nFsaWMoHQbfsc3lIEKTdlPwrRh1fExPW6rSUjg+BgQAIAiLIgNMEppr6gEAAAAZAAAAECi1L/0gEIEAAAoAAAkABG1hcHMKAAAAAAA=",
  "locked": 0
}
⚠️ worldBytes is Base64 encoded!
```
## Get World Names
```
Endpoint: /getWorldNames
Method: GET
Usage: http://api.example.com/getWorldNames
Response format: 200 OK (json):
[
  "exampleworld",
  "exampleworld2",
  "exampleworld3"
]
```
## Get Biggest Worlds
```
Endpoint: /getBiggestWorlds
Method: GET
Usage: http://api.example.com/getBiggestWorlds
Sorted: true (Highest value -> Lowest value)
Response format: 200 OK (json):
[
  {
    "Key": "exampleworld",
    "Value": 840708
  },
  {
    "Key": "exampleworld2",
    "Value": 4210
  },
  {
    "Key": "exampleworld3",
    "Value": 580
  }
]
```
## Create (Insert) World
```
Endpoint: /insertWorld
Method: POST
Usage: http://api.example.com/insertWorld
Body type: JSON
Body format:
{
  "name": "exampleworld4",
  "worldBytes": "sQsJBv//AAAAAgABAwAAAV0AAFsrKLUv/WArWp0KAKKNMTFwa6MA+Nb/////////jQbSM3tZO2Hko/zi1h2icbHY2MQSmhwgfMGSkYhbmkwiu6UP3weCIOg4Hg7lanfphmE1M6B3mxloT6NyF7p32ouBhX9NVq0FxQIKHoTHT78QDMPQ8ZyvtjGfIZdqnaUZ1UZER5qsbpj0JoZAwr/VpkKXmTajnLaZTDfTeoZ8RPA7Ks7/82MoComqZLQkxcG6PEyxYpBWvEhECIArxsoiLYmaYmTJFcrBT02S5ZrAyIbqihWTJR8NfTEgAAOKAKg0DgAU0PUvy+5U+PT4lPEk6WmEG7CK9+ta7DYVibqLqG6oLK5f2XztZpONobc07sgI2N5KlqohEU6CpkJcIw0gPRVwgQcwRFWG60JcG4JefuArvb5Bq2FzaeWnpv7wX1UAY8h7AO/Cx99/98H/FW7+r39SlqqI7loJirqs9FSVIip0AgAAABoAAAARKLUv/SARiQAACgAACQAFdGlsZXMKAAAAAAAAAAAAcgAAAIMotS/9IINNAwCiRhYbgLkZAEAKubeyfTsUe1QwAAsYZmB6CGMMWekHny0Lq4waAuOKzWUlhNxoy1gcRTkJDvhHJH8CNakY8nFsaWMoHQbfsc3lIEKTdlPwrRh1fExPW6rSUjg+BgQAIAiLIgNMEppr6gEAAAAZAAAAECi1L/0gEIEAAAoAAAkABG1hcHMKAAAAAAA=",
  "locked": 0
}
⚠️ worldBytes is Base64 encoded!
Response: 200 OK
```
## Save World
```
Endpoint: /saveWorld
Method: POST
Usage: http://api.example.com/saveWorld
Body type: JSON
Body format:
{
  "name": "exampleworld2",
  "worldBytes": "sQsJBv//AAAAAgABAwAAAV0AAFsrKLUv/WArWp0KAKKNMTFwa6MA+Nb/////////jQbSM3tZO2Hko/zi1h2icbHY2MQSmhwgfMGSkYhbmkwiu6UP3weCIOg4Hg7lanfphmE1M6B3mxloT6NyF7p32ouBhX9NVq0FxQIKHoTHT78QDMPQ8ZyvtjGfIZdqnaUZ1UZER5qsbpj0JoZAwr/VpkKXmTajnLaZTDfTeoZ8RPA7Ks7/82MoComqZLQkxcG6PEyxYpBWvEhECIArxsoiLYmaYmTJFcrBT02S5ZrAyIbqihWTJR8NfTEgAAOKAKg0DgAU0PUvy+5U+PT4lPEk6WmEG7CK9+ta7DYVibqLqG6oLK5f2XztZpONobc07sgI2N5KlqohEU6CpkJcIw0gPRVwgQcwRFWG60JcG4JefuArvb5Bq2FzaeWnpv7wX1UAY8h7AO/Cx99/98H/FW7+r39SlqqI7loJirqs9FSVIip0AgAAABoAAAARKLUv/SARiQAACgAACQAFdGlsZXMKAAAAAAAAAAAAcgAAAIMotS/9IINNAwCiRhYbgLkZAEAKubeyfTsUe1QwAAsYZmB6CGMMWekHny0Lq4waAuOKzWUlhNxoy1gcRTkJDvhHJH8CNakY8nFsaWMoHQbfsc3lIEKTdlPwrRh1fExPW6rSUjg+BgQAIAiLIgNMEppr6gEAAAAZAAAAECi1L/0gEIEAAAoAAAkABG1hcHMKAAAAAAA=",
  "locked": 1615851784994
}
⚠️ worldBytes is Base64 encoded!
Response: 200 OK
```
## Delete World
```
Endpoint: /deleteWorld
Method: POST
Query parameters: worldName (string)
Usage: http://api.example.com/deleteWorld?worldName=exampleworld1
Response: 200 OK
```
## Update Session Lock
```
Endpoint: /updateLock
Method: POST
Query parameters: worldName (string), time (long in milliseconds)
Usage: http://api.example.com/updateLock?worldName=exampleworld1&time=1615851784994
Response: 200 OK
```
## Unlock World
```
Endpoint: /unlockWorld
Method: POST
Query parameters: worldName (string)
Usage: http://api.example.com/unlockWorld?worldName=exampleworld1
Response: 200 OK
```
## Is World Locked
```
Endpoint: /isWorldLocked
Method: GET
Query parameters: worldName (string)
Usage: http://api.example.com/isWorldLocked?worldName=exampleworld1
Response: 200 OK (boolean in number)
1 if locked
0 if not
```

# Planned features
* Expand to multiple databases (MySQL and Local file)
* Release a plugin which utilizes this application
