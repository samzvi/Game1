# Tank Strategy Game - Game Design Document

## Overview

A competitive 1v1 online tactical board game focused on prediction, planning, and psychology.

Players control a small number of tanks on a grid battlefield. Both players have full information about the battlefield, but hidden intentions. Each turn players secretly plan actions, then all actions are resolved simultaneously.

The goal is not randomness or reaction speed. The goal is outthinking the opponent.

---

# Design Philosophy

The game should reward:

- Prediction
- Strategic thinking
- Reading the opponent
- Planning ahead
- Positioning

The game should avoid:

- Random damage
- Random outcomes
- Luck deciding victories
- Excessive complexity

A player should lose because the opponent made better decisions.

The ideal feeling:

> "I knew what he would do, but he knew that I knew."

The closest inspirations are:

- Chess:
  - Perfect information
  - Deep strategic thinking
  - No randomness
- Valorant:
  - Positioning
  - Mind games
  - Reading opponents
- osu!:
  - Pure skill improvement
  - Deterministic results

---

# Core Gameplay

## Board

- Square grid battlefield
- Default size: 15x15 tiles
- Board size may become configurable as difficulty setting

Players fight on the same shared battlefield.

---

# Players

- 2 players only
- Each player controls 2 tanks

Each tank has:

- Name
- Position
- Facing direction
- Health

Tank names are generated from the Greek alphabet:

Examples:
- Alpha
- Beta
- Gamma
- Delta
- Omega

The purpose is to make tanks feel like individual units rather than anonymous pieces.

---

# Deployment Phase

Before the first turn:

- Players secretly choose starting positions.
- Tanks are placed inside their starting area.
- Starting positions are hidden until both players confirm.
- Players choose initial facing direction.

Possible starting area:
- First 2-3 rows of the player's side.

---

# Turn System

The game is simultaneous.

Players do not directly execute actions.

Instead:

1. Player selects actions for each tank.
2. Player commits the turn.
3. Opponent does the same.
4. Server resolves both plans.
5. New game state is sent to both players.

---

# Actions

Possible tank actions:

## Move

Moves the tank by one tile.

## Rotate

Rotates tank by 90 degrees.

## Fire

Shoots in a straight line.

---

# Action Points

The exact AP system is still being tested.

Current preferred direction:

- Each round gives a fixed number of action points.
- Each action consumes one action point.
- Actions can be assigned freely between tanks.

Example:

2 AP:

Alpha:
- Move

Beta:
- Fire


Possible future variations:

- 3 AP per turn
- Different AP distribution systems
- Temporary bonuses

Avoid systems that create large snowballs.

---

# Action Queue UI

Players queue actions before committing.

Example:

Alpha:

[Move Forward] [Rotate Right] [Fire]


UI concept:

- Select tank
- Action menu appears
- Selected actions appear above the tank as bubbles/icons
- Actions can be removed individually
- Actions are ordered and executed sequentially

The player should feel like they are programming the tank's turn.

---

# Turn Resolution

Resolution must always be deterministic.

Recommended order:

1. Movement
2. Rotation
3. Shooting
4. Damage calculation
5. Remove destroyed tanks

The same game state with the same actions must always produce the same result.

---

# Combat

## Shooting

- Straight line projectile
- Unlimited range
- Direction based
- No accuracy randomness

Possible future terrain:
- Obstacles block shots

---

# Armor / Health

Current idea:

Directional armor.

Front:
- Can survive 2 hits

Side/back:
- Destroyed after 1 hit

The purpose:

- Make facing direction meaningful
- Encourage positioning
- Make rotation important

Avoid traditional large health pools.

---

# Victory

A player loses when all tanks are destroyed.

Possible future alternate modes:

- Control points
- Capture objectives
- Timed survival
- Ranked scenarios

---

# Randomness

The game should contain minimal randomness.

Avoid:

- Random damage
- Random accuracy
- Random critical hits

Possible acceptable randomness:

- Cosmetic elements
- Procedural map generation if both players have equal information
- Symmetric events

---

# Map Design

Current idea:

- Main obstacles are initially the grid boundaries.

Possible future additions:

- Walls
- Destructible objects
- Cover
- Terrain effects

Any terrain must improve strategic decisions, not add complexity only.

---

# Neutral Objectives / Powerups

Possible future mechanic:

Objects spawning near the center of the map.

Purpose:
- Force engagement
- Prevent passive play

Examples:

- Temporary +1 AP next turn
- Repair +1 HP
- Temporary armor
- Special ability

Avoid permanent advantages that create snowballing.

---

# Multiplayer Architecture

## Technology

Recommended:

- Blazor WebAssembly frontend
- ASP.NET Core backend
- SignalR realtime communication

---

# Architecture Principles

The server is authoritative.

The client should never decide:

- Valid moves
- Damage
- Victory
- Game state

The client only sends intentions:

Example:
Tank: Alpha
Action: Move
Direction: Forward


Server:

1. Validates action
2. Resolves turn
3. Updates state
4. Broadcasts result

---

# Suggested Solution Structure


TankGame

TankGame.Web

Blazor WebAssembly UI
Board rendering
Player interaction
SignalR client

TankGame.Server

ASP.NET Core
SignalR Hub
Room management
Match handling

TankGame.Game

Pure game logic
Board
Tanks
Actions
Turn resolver

The game logic project must not depend on:
- Blazor
- SignalR
- HTTP

It should be testable independently.

---

# Rooms

Game flow:

## Create

Player enters:

- Nickname
- Game options

Server creates:

- Room ID
- PIN
- Shareable link


Example:


game.samzvi.site/r/ABC123


---

## Join

Player enters:

- Nickname
- PIN

Then joins existing room.

---

# Reconnection

Players should be able to reconnect.

Recommended:

- Browser stores player token.
- Username is only cosmetic.

Do not rely only on:


nickname + PIN


for identity.

---

# Future Features

## Match history

Store:

- Players
- Result
- Date
- Duration


## Replay system

Because the game is deterministic, store:

- Initial state
- Player actions
- Turn results

Allow replaying any match.

---

## Rankings

Possible:

- Elo rating system

Because results are deterministic, ranking is meaningful.

---

## Spectator mode

Possible because server already owns complete state.

---

# Important Development Rule

Do not add mechanics unless they improve meaningful decision making.

A simple game with deep decisions is preferred over a complicated game with shallow interactions.

The core question:

> "Can I predict my opponent better than they can predict me?"