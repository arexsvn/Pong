# Pong World Tour

## How to play
Play the game on itch.io at https://arexsvn.itch.io/pong

The game can be played in a mobile or desktop browser or by running a native executable. A Windows build is included in this repo at BuildArchives/Pong_build_win.zip. 

To build the game locally clone the repo and add as a new project in Unity like usual (I'm using unity 2022.3.1f1, this version or newer should work fine.) The default WebGL or Windows/Mac/Linux platforms can be tested with the usual "Build and Run" button in Build Settings. To test multiplayer locally simply run multiple versions of the .exe.

Crossplay between all platforms is supported including native app and WebGL instances. A game or dedicated server can be started from any platform.
### Multiplayer
A multiplayer game can be started by either hosting a game (Play Game > Versus Human > Host Game) or starting a dedicated server (Play Game > Start Dedicated Server).
Either option will start a multiplayer game and provide a room code when it is ready to be joined. A second player can join via Play Game > Versus Human > Join Game and entering the same room code. If running a dedicated server a total of three games must be started, the dedicated server instance and two clients that join the server with the same roomcode.
### VS Ai
A local game can be played against a CPU-controlled opponent by clicking Play Game > Versus Ai
### Controls
The mallet can be moved left and right by holding the mouse down / tapping and holding the left or right sides of the screen.
The keyboard arrow keys can also be used for movement if playing on a desktop.
To 'serve' the puck either tap/click anywhere or press the spacebar.
### Other Features
* Tap the airplane button on the main menu to visit exotic locales with different color themes!
* Music and SFX volume can be adjusted from the settings menu (gear icon on the main menu)

## Implementation Overview
The default project platform is WebGL. Unity Relay is used as the default multiplayer transport but traditional Unity Transport is also implemented. It can be activated by creating a build with the ALLOW_NON_RELAY_MULTIPLAYER conditional compilation flag set. I created an in-editor utility to make this easier (Windows > Defines Panel). Once activated there are additional options for starting a multiplayer game labeled as "non-relay". Joining a non-relay game requires manually entering the IP Address of the host machine. 
* WebGL builds cannot host or start a dedicated server in this mode.

I used a game I previously created independently and released on Android called 'Word Tour' as a starting point for this project. The existing project includes some UI systems, audio, UI assets, and general app structure including dependency injection for sharing services (not used for gameplay.)

All of the gameplay code and multiplayer functionality were created from scratch for this project. That code primarily exists in Assets/Runtime/Game.

## Implementation Details
* AppController - This is the entry point of the application and is responsible for initializing ui and game systems.
* GameController - Controls the overall gameplay flow for local and multiplayer games.
* NetworkGameManager - Handles interaction with Unity netcode including multiplayer game startup and client<>server messaging. Multiplayer games are server authoritative with the client sending only inputs. The server then updates the positions of the players and puck (Ball) based on the client's requests to move/serve. High-level game events are handled by sending "custom unnamed messages" which are defined by a message "type" and an optional value for a payload. This is used to handle updating clients with game start/restart and scoring events.

Client input requests are handled with RPC messages in the "Networked" version of the gameplay components for Player and Ball.

While this setup works decently well the client interpolation/server input processing introduces latency to the movement of the puck and mallets. A potential solution for this could be to use client extrapolation and local input handling where the client predicts the placement of gameplay objects. This would work well for the movement of the puck since it follows a predictable path but would require state synchronization to deal with any client divergence from the authoritative state on the server.

* LocalGameManager - Handles starting and running a local game. 

Local and Networked gameplay objects share common functionality in Player and Ball components and handle unique functionality in the corresponding 'Local' and 'Networked' versions of those components.

VContainer (https://vcontainer.hadashikick.jp/) is the dependency injection system used for setting up and sharing dependencies between services (called controllers in this project :)). It solves some of the issues with older DI systems like high allocations and slow resolve time. 

The game loads assets using simple unity Resource methods. I used this approach simply because my previous project that I used as a starting point did but I much prefer using Addressables for content loading and management due to the simplicity of switching between remote and local assets. For larger projects, I find it useful to split between a minimal, code-only application project that loads content via remote addressables created from the content project(s).
