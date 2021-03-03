
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
Several photo stocks to choose from, over provided musical genres (or Spotify playlists).

[![XQ4wGcetf7.md.gif](https://s4.gifyu.com/images/XQ4wGcetf7.md.gif)](https://gifyu.com/image/ZZNO)

You need to create:
* MySQL Database, a user with `SUPER` global priviliegis: `DB_USER=user` `DB_PASS=pass` `DB_NAME=database` and COPY to `EF_USER=user` `EF_PASSWORD=pass` `EF_DATABASE=database` (debugging) in the `.env` file (or pass docker env)

* Fill tables with data `Playlists` - contains Spotify Playlist Uri (see screenshot) and its name.
 `console_Photostocks` - contains URLs, such as https://www.deviantart.com/topic/* (replace * with any topic name)
[![16d1981c7ca862232.png](https://s4.gifyu.com/images/16d1981c7ca862232.png)](https://gifyu.com/image/ZZtZ)
[![2e324a62384c06473.png](https://s4.gifyu.com/images/2e324a62384c06473.png)](https://gifyu.com/image/ZZtV)
[![3d0a9f6bd38d856a8.md.png](https://s4.gifyu.com/images/3d0a9f6bd38d856a8.md.png)](https://gifyu.com/image/ZZ53)
* Socks5Proxy (to download images from Devianart and don't get banned), you can use Tor Browser (run it and `TOR_HOST=localhost` `TOR_PORT=9150`)  to debug (DONT use it in production)
* VK user account (without 2FA, just phone number and password)
* Make sure that this user has `Community admin` rights ( ex. https://vk.com/piblic12345?act=users&tab=admins)
* Log in user and open page https://oauth.vk.com/authorize?client_id=2685278&scope=1073737727&redirect_uri=https://oauth.vk.com/blank.html&display=page&response_type=token&revoke=1 copy ->  `KATE_MOBILE_TOKEN` (need to upload attachments)
* Create Standalone APP https://vk.com/editapp?act=create -> pass App ID here (`YOURAPPID`) https://oauth.vk.com/authorize?client_id=YOURAPPID&scope=notify,photos,friends,audio,video,notes,pages,docs,status,questions,offers,wall,groups,notifications,stats,ads,offline&redirect_uri=http://api.vk.com/blank.html&display=page&response_type=token then open page copy and paste &token=`....` -> `TOKEN` (need to upload wall post as your APP)
* `OWNER_ID` - Admin user ID (Go to any wall past -> click on date https://vk.com/user?w=wall-11111111_22222  - 11111111 is your ID, copy and paste it )
* `GROUP_ID` - Community ID  (Go to any wall past -> click on date https://vk.com/community?w=wall-11111111_22222  - 11111111 is your ID )
* `CLIENT_ID` and `CLIENT_SECRET` from Spotify Developers https://developer.spotify.com/dashboard/applications (create app to parse Spotify playlists)
* `HOURS_PERIOD` and `MINUTES_PERIOD` - delay between postponed posts
* `URL` - from https://api-vk.com/ -> `Ключ` (need to download music)
* `RABBIT_HOST` - your RabbitMQ server (to send to frontend) (IN DEVELOPMENT)
* `REDIS_HOST=localhost` `REDIS_PORT=6479` `REDIS_PASSWORD=pass` - currently not used (fill randomly or pass like that)

## Readme:
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

## ENV File or Docker ENV
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
KATE_MOBILE_TOKEN=vk
TOKEN=vk
URL=apiws
OWNER_ID=1111111
GROUP_ID=1111111
CLIENT_ID=1111111
CLIENT_SECRET=11111111
ADMIN_ID=OWNER_ID
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
