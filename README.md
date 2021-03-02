
# vkaudioposter_Console
![](https://img.shields.io/github/followers/rhiskey?style=social)
![forks](https://img.shields.io/github/forks/rhiskey/vkaudioposter_Console?style=social)
![stars](https://img.shields.io/github/stars/rhiskey/vkaudioposter_Console?style=social)

![issues](https://img.shields.io/github/issues/rhiskey/vkaudioposter_Console)
![](https://img.shields.io/github/issues-closed-raw/rhiskey/vkaudioposter_Console)
![](https://img.shields.io/github/license/rhiskey/vkaudioposter_Console)
![tag](https://img.shields.io/github/v/tag/rhiskey/vkaudioposter_Console)
![release](https://img.shields.io/github/v/release/rhiskey/vkaudioposter_Console)
![contrib](https://img.shields.io/github/contributors/rhiskey/vkaudioposter_Console)
![lastcommit](https://img.shields.io/github/last-commit/rhiskey/vkaudioposter_Console)
![lang](https://img.shields.io/github/languages/count/rhiskey/vkaudioposter_Console)
![](https://img.shields.io/github/commit-activity/m/rhiskey/vkaudioposter_Console)
![checks](https://img.shields.io/github/checks-status/rhiskey/vkaudioposter_Console/main)


A program for generating music posts in various genres for VKontakte.
Several photo stocks to choose from, over 100 musical genres.

# Readme:
1. Be sure to copy `.env` to Debug and Release
2. Run RabbitMQ `docker run -it --rm --name rabbitmq -p 5672: 5672 -p 15672: 15672 rabbitmq: 3-management`
3. Exclude the `tests` folder from the main project
4. Check hosts in MYSQL if it is possible to connect from IP
5. Check `.env` file: RABBIT_HOST address and DB_HOST
6. ` docker build --tag hvm-csharp .`
7. ` docker run -d -i --restart on-failure --network hvm hvm-csharp`
8. Consider the TorProxy and RabbitMQ address when creating containers
9. You should run your container in Interactive mode (with the -i option), but please note that the background processes will be closed immediately when you run the container, so make sure your script is run in the foreground or it simply won ' t work.
10. `vkaudioposter-ef` build as class library, specify Folder profile offline build .netCore Linux_x64
11. In the `.csproj` of the current project
```
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net5.0</TargetFramework>
    <GenerateTargetFrameworkAttribute>false</GenerateTargetFrameworkAttribute>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
  </PropertyGroup>
```
12. `docker build . -f Dockerfile.debian-x64`

# ENV File or Docker ENV
.env:
```
HOURS_PERIOD=2
MINUTES_PERIOD=20
DB_HOST=localhost
RABBIT_HOST=localhost
TOR_HOST=localhost
TOR_PORT=9150
DB_USER=user
DB_PASS=pass
DB_NAME=database
ACCESS_TOKEN=vk
KATE_MOBILE_TOKEN=vk
TOKEN=vk
URL=apiws
OWNER_ID=1111111
GROUP_ID=1111111
CLIENT_ID=1111111
CLIENT_SECRET=11111111
USER_ACCESS_TOKEN=vk
ADMIN_ID=1111111
#PUSHER_APP_ID=111111
#PUSHER_APP_KEY=111111
#PUSHER_APP_SECRET=11111
AUTO_START=false
START_ONCE=true
SAVE_LOGS=true
REDIS_HOST=localhost
REDIS_PORT=6479
REDIS_PASSWORD=pass
EF_USER=user
EF_PASSWORD=pass
EF_DATABASE=db
```
