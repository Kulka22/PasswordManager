# PasswordManager

## Как забилдить проект:
### 1. Переходим в папку с проектом.
### 2. Выполняем команду: dotnet publish PasswordManager.WPF -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true /p:PublishTrimmed=false
### 3. В результате чего получаем единый exe-файл.
### 4. Он находится тут: PasswordManager.WPF\bin\Release\net6.0-windows\win-x64\publish\.
