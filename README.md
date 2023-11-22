# Pong World Tour

## How to play
Play the game on itch.io at https://arexsvn.itch.io/pong
Game can be played in a mobile or desktop browser.
A game can also be played by running a native executable. A windows build is included in this repo at BuildArchives/Pong_build_win.zip.
Crossplay between all platforms is supported including native app and WebGL instances. A game or dedicated server can be started from any platform.
### Multiplayer
A multiplayer game can be started by either hosting a game (Play Game > Versus Human > Host Game) or starting a dedicated server (Play Game > Start Dedicated Server).
Either option will starta multiplayer game and provide a room code when it is ready to be joined. A second player can join via Play Game > Versus Human > Join Game and entering the same room code. If running a dedicated server a total of three games must be started, the dedicated server instance and two clients that join the server with the same roomcode.
### VS Ai
A local game can be played against a CPU controlled opponent by clicking Play Game > Versus Ai
### Controls
The mallet can be moved left and right by holding the mouse down / tapping and holding the left or right sides of the screen.
The keyboard arrow keys can also be used for movement if playing on a desktop.
To 'serve' the puck either tap/click anywhere or press the spacebar.
### Other Features
* Tap the airplane button on the main menu to visit exotic locales with different color themes.
* Music and SFX volume can be adjusted from the settings menu (gear icon on main menu)

## Implementation
The default project platform is WebGL. Unity Relay is used as the default multiplayer transport but traditional Unity Transport is also implemented. It can be activated by creating a build with the ALLOW_NON_RELAY_MULTIPLAYER conditional compilation flag set. I created a in-editor utility to make this easier (Windows > Defines Panel). Once activated there are additional options for starting a multiplayer game labeled as "non-relay". Joining a non-relay game requires manually entering the IP Address of the host machine. 
* WebGL builds cannot host or start a dedicated server in this mode.

I used a game I previously created as a starting point for this project. The existing project including some UI systems and the general app structure including dependency injection for sharing services (not used for gameplay.)

All of the gameplay code and multiplayer functionality was created from scratch for this project. That code primarily exists in Assets/Runtime/Game.
