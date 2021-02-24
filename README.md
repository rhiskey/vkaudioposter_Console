# vkaudioposter_Console
![](https://img.shields.io/github/commit-activity/m/rhiskey/vkaudioposter_Console)
Программа для генерации музыкальных постов в различных жанрах для ВКонтакте. 
Несколько фотостоков на выбор, более 100 музыкальных жанров.

1. Обязательно скопировать .env в Debug и Release 

2. Запустить RabbitMQ `docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management`

3. Исключить из основного проекта папку `tests`

4. Проверить хосты в MYSQL, есть ли возможность подключаться с IP 
5. Проверить .env файл: RABBIT_HOST адрес и DB_HOST

6. `docker build --tag hvm-csharp .`
7. `docker run -d -i --restart on-failure --network hvm hvm-csharp`

8. Учесть адрес TorProxy и RabbitMQ при создании контейнеров 
9. You should run your container in Interactive mode (with the -i option), but please note that the background processes will be closed immediately when you run the container, so make sure your script is run in the foreground or it simply won't work.
10. vkaudioposter-ef собирать как библиотеку классов, указать профиль Folder автономная сборка .netCore Linux_x64
11. В .csproj текущего проекта 

```
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net5.0</TargetFramework>
    <GenerateTargetFrameworkAttribute>false</GenerateTargetFrameworkAttribute>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
  </PropertyGroup>
```

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
```