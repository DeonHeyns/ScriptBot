SkypeBot
===================================

So there is Hubot and we all know its super awesome and used extensively by the good people at GitHub. I was wondering how hard would it be to write something similar using C# and using Skype as the Chat client. Well here it is and to be honest it wasnt that difficult. 

So I present to you SkypeBot it uses MEF to load different plugins you can use to make your robot do tasks and there is an implementation using Scriptcs. 

#Prereqs
You will need to download Skype and register the skype4COM.dll. You can do this by running "regsvr32 %programfiles(x86)%\Common Files\Skype\Skype4COM.dll" on 64bit machines and "regsvr32 %programfiles%\Common Files\Skype\Skype4COM.dll" on 32bit machines.

You will then want to install Scriptcs. The easiest way to install and use ScriptCS is to use [Chocolatey](http://www.chocolatey.org/).
Install Chocolatey using: @powershell -NoProfile -ExecutionPolicy unrestricted -Command "iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))" && SET PATH=%PATH%;%systemdrive%\chocolatey\bin

and then

cinst ScriptCS.