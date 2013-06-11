SkypeBot
===================================

So there is Hubot and we all know its super awesome and used extensively by the good people at GitHub. I was wondering how hard would it be to write something similar using C# and using Skype as the Chat client. Well here it is and to be honest it wasnt that difficult. 

So I present to you SkypeBot it uses MEF to load different plugins you can use to make your robot do tasks and there is an implementation using Scriptcs. 

#Prereqs
You will need to download Skype and register the skype4COM.dll. You can do this by running "regsvr32 %programfiles(x86)%\Common Files\Skype\Skype4COM.dll" on 64bit machines and "regsvr32 %programfiles%\Common Files\Skype\Skype4COM.dll" on 32bit machines.

Following that you will want to create a Skype account for your bot and add it as a contact and a member of any Skype room you may have. 

Then you will want to install Scriptcs. The easiest way to install and use ScriptCS is to use [Chocolatey](http://www.chocolatey.org/).
Install Chocolatey using: @powershell -NoProfile -ExecutionPolicy unrestricted -Command "iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))" && SET PATH=%PATH%;%systemdrive%\chocolatey\bin

and then

cinst ScriptCS.

#How to get going

You have two choices at creating functionality for SkypeBot you can right MEF plugins or you can just write a Scriptcs file drop it into the scriptcs folder where SkypeBot is running. To call your plugin simply type @bot <command> directly to the SkypeBot or in the room where SkypeBot is a member. The real power comes from using the built in Scriptcs plugin. Once you have written your Scriptcs script and copied it into the scriptcs folder just tell SkypeBot @bot scriptcs <scriptcs file>. SkypeBot will run the file and display the output in your Skype chat.