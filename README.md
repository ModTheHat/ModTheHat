# ModTheHat
ModTheHat: Modloader for King of the Hat

## How it works

We use a tried and proven technique that other modloaders for Unity games use. Which is by using [MonoMod](https://github.com/MonoMod/MonoMod/blob/master/README.md).

## How to compile:

Requirements:
 - The Game
 - MonoMod (A version is included)
 - .NET 5.0 (Required for MonoMod)
 - Visual Studio

First step is to update the libs folder. This folder is required to compile our custom Assembly-CSsharp

Copy the libraries from the 'C:\Program Files (x86)\Steam\steamapps\common\King of the Hat\KingOfTheHat_Data\Managed' folder into libs

Open the project solution in Assembly-CSharp/Assembly-CSharp.sln in Visual Studio.

Build the project.

Copy the DLL in Assembly-CSharp/Assembly-CSharp/bin/Release/Assembly-CSharp.Base.mm.dll to the game folder.

Run the MonoMod.exe file in this folder

Rename the 'MONOMODDED_Assembly-CSharp.dll' to 'Assembly-CSharp.dll'

Have fun!