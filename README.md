**Player Administration** provides a simple-to-use GUI that helps admins moderate users.

## Features

- Banning/unbanning users
- Kicking users
- Killing users
- Muting/unmuting a player's voice chat
- Muting/unmuting a player's text chat
- Clearing the user's inventory
- Resetting a user's blueprints
- Resetting a user's metabolism
- Hurting a user
- Healing a user
- The ability to see a user's vitals, status and steamID64

## Permissions

- **playeradministration.show** -- Required to be able to use the `/padmin` command and plugin

## Chat Commands

- **/padmin** -- Show the player administration menu ***(requires `playeradministration.show` permission)***

## Configuration

```json
{
  "Enable kick action": true,
  "Enable ban action": true,
  "Enable unban action": true,
  "Enable kill action": true,
  "Enable inventory clear action": true,
  "Enable blueprint reset action": true,
  "Enable metabolism reset action": true,
  "Enable hurt action": true,
  "Enable heal action": true,
  "Enable voice mute action": true,
  "Enable voice unmute action": true,
  "Enable chat mute action": true,
  "Enable chat unmute action": true
}
```

## Localization

The default messages are in the `PlayerAdministration.json` file under the `oxide/lang/en` directory. To add support for another language, create a new language folder (ex. de for German) if not already created, copy the default language file to the new folder, and then customize the messages.

```json
{
  "Permission Error Text": "You do not have the required permissions to use this command.",
  "Permission Error Log Text": "{0}: Tried to execute a command requiring the '{1}' permission",
  "Kick Reason Message Text": "Administrative decision",
  "Ban Reason Message Text": "Administrative decision",

  "Never Label Text": "Never",
  "Banned Label Text": " (Banned)",
  "Dev Label Text": " (Developer)",
  "Connected Label Text": "Connected",
  "Disconnected Label Text": "Disconnected",
  "Sleeping Label Text": "Sleeping",
  "Awake Label Text": "Awake",
  "Alive Label Text": "Alive",
  "Dead Label Text": "Dead",
  "Flying Label Text": " Flying",
  "Mounted Label Text": " Mounted",

  "User Button Page Title Text": "Click a username to go to the player's control page",
  "User Page Title Format": "Control page for player '{0}'{1}",

  "Ban By ID Title Text": "Ban a user by ID",
  "Ban By ID Label Text": "User ID:",
  "Player Info Label Text": "Player information:",
  "Player Actions Label Text": "Player actions:",

  "Id Label Format": "ID: {0}{1}",
  "Auth Level Label Format": "Auth level: {0}",
  "Connection Label Format": "Connection: {0}",
  "Status Label Format": "Status: {0} and {1}",
  "Flags Label Format": "Flags:{0}{1}",
  "Position Label Format": "Position: {0}",
  "Rotation Label Format": "Rotation: {0}",
  "Last Admin Cheat Label Format": "Last admin cheat: {0}",
  "Idle Time Label Format": "Idle time: {0} seconds",
  "Health Label Format": "Health: {0}",
  "Calories Label Format": "Calories: {0}",
  "Hydration Label Format": "Hydration: {0}",
  "Temp Label Format": "Temperature: {0}",
  "Wetness Label Format": "Wetness: {0}",
  "Comfort Label Format": "Comfort: {0}",
  "Bleeding Label Format": "Bleeding: {0}",
  "Radiation Label Format": "Radiation: {0}",
  "Radiation Protection Label Format": "Protection: {0}",

  "Main Tab Text": "Main",
  "Online Player Tab Text": "Online Players",
  "Offline Player Tab Text": "Offline Players",
  "Banned Player Tab Text": "Banned Players",

  "Clear Inventory Button Text": "Clear Inventory",
  "Reset Blueprints Button Text": "Reset Blueprints",
  "Reset Metabolism Button Text": "Reset Metabolism",

  "Hurt 25 Button Text": "Hurt 25",
  "Hurt 50 Button Text": "Hurt 50",
  "Hurt 75 Button Text": "Hurt 75",
  "Hurt 100 Button Text": "Hurt 100",

  "Heal 25 Button Text": "Heal 25",
  "Heal 50 Button Text": "Heal 50",
  "Heal 75 Button Text": "Heal 75",
  "Heal 100 Button Text": "Heal 100",

  "Ban Button Text": "Ban",
  "Kick Button Text": "Kick",
  "Kill Button Text": "Kill",
  "Unban Button Text": "Unban",

  "Voice Mute Button Text": "Mute Voice",
  "Voice Unmute Button Text": "Unmute Voice",
  "Chat Mute Button Text": "Mute Chat",
  "Chat Unmute Button Text": "Unmute Chat"
}
```
