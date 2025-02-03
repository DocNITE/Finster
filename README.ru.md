# NetPunk

<p align="center"><img src="./Raw/Screenshots/1.png"  /></p>

[![en][icon-en]][en] [![ru][icon-ru]][ru]

[en]: ./README.md
[icon-en]: https://img.shields.io/badge/lang-en-red?style=flat-square
[ru]: ./README.ru.md
[icon-ru]: https://img.shields.io/badge/lang-ru-orange?style=flat-square

Основная задача создания новой полноценной игры на основе существующего контента для Space Station 14, с упором в уникальный лор и ролевую составляющую.

Проект некомерческий и развивается на собственном энтузиазме. Однако для связи существует собственный [Discord](https://discord.gg/Fmzp3kQ3AB) сервер.

## Ссылки

[Steam](https://store.steampowered.com/app/1255460/Space_Station_14/) | [Steam (SSMV Launcher)](https://store.steampowered.com/app/2585480/Space_Station_Multiverse/) | [Клиент без Steam (SSMV Launcher)](https://spacestationmultiverse.com/downloads/)  | [Flathub](https://flathub.org/apps/com.spacestation14.Launcher) |  [Основной(Upstream) репозиторий](https://github.com/Simple-Station/Einstein-Engines) | [Discord сервер проекта](https://discord.gg/Fmzp3kQ3AB)

## Контрибуция

Для добавления больших изменений в игру рекомендуется создание отдельного Pull Request'а.
Для мелких правок достаточно обычного commit'а в **slave**.

Нужно учитывать, что репозиторий представляет из себя **"солянку"**, собранную из разных открытых ресурсов на платформе.
Любой добавленный не будет защищаться на контрибьютером, ни автором проекта.

## Сборка

Следуйте [гайду от Space Wizards](https://docs.spacestation14.com/en/general-development/setup/setting-up-a-development-environment.html) по настройке рабочей среды, но учитывайте, что репозиторий отличается и некоторые вещи могут также отличаться.
EE предлагает несколько скриптов, показанных ниже, чтобы облегчить работу.

### Необходимые зависимости

> - Git
> - .NET SDK 9.0.101

### Windows

> 1. Склонируйте данный репозиторий
> 2. Запустите `git submodule update --init --recursive` в командной строке, чтобы скачать движок игры
> 3. Запускайте `Tools/bat/buildAllDebug.bat` после любых изменений в коде проекта
> 4. Запустите `Tools/bat/runQuickAll.bat`, чтобы запустить клиент и сервер
> 5. Подключитесь к локальному серверу и играйте

### Linux

> 1. Склонируйте данный репозиторий.
> 2. Запустите `git submodule update --init --recursive` в командной строке, чтобы скачать движок игры
> 3. Запускайте `Tools/sh/buildAllDebug.sh` после любых изменений в коде проекта
> 4. Запустите `Tools/sh/runQuickAll.sh`, чтобы запустить клиент и сервер
> 5. Подключитесь к локальному серверу и играйте

### MacOS

> Аналогично как и на Linux. В ином случае используйте `dotnet build` для сборки проекта.

## License

👉 [LEGAL.md](./LEGAL.md)
