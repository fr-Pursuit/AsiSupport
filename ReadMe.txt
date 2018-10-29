I) Installation

	In order to install AsiSupport, you must extract the content of the "Installation" folder in your "Grand Theft Auto V" folder.
	
	To use the plugin, you must also install:
	 - .NET Framework 4.6: https://www.microsoft.com/en-us/download/details.aspx?id=48130
	 - RAGE Plugin Hook: http://ragepluginhook.net/
	
	Once you've verified you have all required dependencies, launch the game through RagePluginHook.exe

II) How does it work?

	The first version was entirely coded in C++/CLI: native C++ code was used to interact with native ASI plugins, and the "CLI" (or "managed") part of the code was used to interact with the .NET Framework and RAGE Plugin Hook.
	Though, this mix between managed and native code created problems and confusion: errors were hard to track down, the program was hard to maintain, and the plugin didn't work for a lot of people.
	
	This version was created with compatibility and user-friendliness in mind: managed code is in its own file ("AsiSupport.dll", which is the RPH Plugin), and all unmanaged code (the bare minimum the make the plugin work) is in
	UnvAsiIntrf.dll (which stands for "Universal ASI Interface"). UnvAsiIntrf.dll should be supported by the end user as it is pure C++, and AsiSupport.dll should also not cause any problem if the end user has all required
	dependencies to use RPH.
	
	UnvAsiIntrf.dll has been created as a generic way for ASI plugins to interact with another entity: said entity would register as an "API Handler" which has access to GTA V (in this instance, to RPH which gives it access to GTA V)
	and which would load UASI files; then, the ASI scripts would interact with GTA V the same way the would've done it with ScriptHookV, except they would do via UnvAsiIntrf.dll, which itself serves as a simple bridge between
	UASI scripts and the API Handler.

III) Credits

	ASI Support for RAGE Plugin Hook
	Created by Pursuit
	
	Thanks to MulleDK19 and LMS for RAGE Plugin Hook
	Thanks to Alexander Blade for ScriptHookV
	Thanks to alexguirre for helping me figure out stuff

IV) License
	
	ASI Support for RAGE Plugin Hook
	Copyright © 2018 Pursuit
	
	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.
	
	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.
	
	You should have received a copy of the GNU General Public License along
	with this program; if not, write to the Free Software Foundation, Inc.,
	51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
