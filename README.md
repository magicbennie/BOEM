# Black Ops Error Monitor (BOEM)

BOEM is a monitoring tool for Call of Duty: Black Ops 1 zombies. It provides a customizable user interface that shows values of interest to HR zombies players.

## Features

 - Displays values like `numSnapshotEntites` (value that causes reset) and `com_frametime` (value that causes 25 day black screen).
 - Customizable UI (toggle displaying of specific values & change colour)
 - SEH hook
 - Entity list
 - Minimal memory/CPU usage

## Download

Prebuilt binaries are available to download on the [official download page](https://download.magicbennie.com/BlackOpsZombies/ErrorMonitor).

## Usage

When run, BOEM will attach to the game and begin providing updates to the displayed values.

Click the `Settings and Layout` button to open the Settings menu, where you can customize the list of displayed values toggle BOEMs other features.

## Todo

General tidy up as this code is 2 years old and needs to be brought into the 21st century.

## Other Tools

Those streaming BO1 Zombies on Twitch may be interested in [TIM](https://download.magicbennie.com/BlackOpsZombies/TIM/) (Twitch Integration Mod), which makes most values of interest accessible from both ingame and Twitch chat.

Other tools are listed [here](https://download.magicbennie.com/).