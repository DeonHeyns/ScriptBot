ScriptBot
===================================

So there is Hubot and we all know its super awesome and used extensively by the good people at GitHub. I was wondering how hard would it be to write something similar using C# and Skype as the Chat client. Well here it is and to be honest it wasnt that difficult. 

So I present to you ScriptBot it uses MEF to load different plugins you can use to make your robot do tasks and there is an implementation using ScriptCs. ScriptBot is extensible you can implement any Transport channel you want.

#Prereqs
You will need to download Skype and register the skype4COM.dll. You can do this by running "regsvr32 %programfiles(x86)%\Common Files\Skype\Skype4COM.dll" on 64bit machines and "regsvr32 %programfiles%\Common Files\Skype\Skype4COM.dll" on 32bit machines.

Following that you will want to create a Skype account for your bot and add it as a contact and a member of any Skype room you may have. 

Then you will want to install ScriptCs. The easiest way to install and use ScriptCs is to use [Chocolatey](http://www.chocolatey.org/).
Install Chocolatey using: @powershell -NoProfile -ExecutionPolicy unrestricted -Command "iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))" && SET PATH=%PATH%;%systemdrive%\chocolatey\bin

and then

cinst ScriptCs.

#How to get going

You have two choices at creating functionality for ScriptBot you can right MEF plugins or you can just write a ScriptCs file drop it into the ScriptCs folder where ScriptBot is running. To call your plugin simply type @bot <command> directly to the ScriptBot or in the room where ScriptBot is a member. The real power comes from using the built in ScriptCs plugin. Once you have written your ScriptCs script and copied it into the ScriptCs folder just tell ScriptBot @bot ScriptCs <ScriptCs file>. ScriptBot will run the file and display the output in your Skype chat.

1.) Download or clone the source

2.) Change the SkypeBotScriptCsCommand:ScriptCsExePath setting if ScriptCs.exe is not in your path and set the  ScriptBot.exe.config

3.) XCopy the built Release files to C:\Program Files (x86)\ScriptBot

4.) Open up CMD as an Administrator

5.) Enter the command ScriptBot.exe install

6.) Enter the command ScriptBot.exe start

If you are using the SkypeTransport then Skype will need to be started ahead of starting the ScriptBot.exe (there is an issue to fix this).

#How to get hold of me
twitter: [@DeonHeyns](https://twitter.com/deonheyns)

website: [deonheyns.com](http://deonheyns.com/contact)

GitHub: [GitHub Profile](https://github.com/deonheyns)

#Open source projects used:

ScriptCs: https://github.com/ScriptCs/ScriptCs

Topshelf: https://github.com/phatboyg/Topshelf