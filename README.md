**Player Administration** provides a simple-to-use GUI that helps admins moderate users.

## Features

- Banning/unbanning users
- Kicking users
- Killing users
- Muting/unmuting a player's voice chat
- Muting/unmuting a player's text chat
- Clearing the user's inventory
- Resetting a user's blueprints
- Resetting a user's metabolism (Identical to how a respawn sets a random metabolism)
- Recover a user's metabolism (Gives the user a healthy metabolic state by filling their hunger, thirst, oxygen, and removing bleeding and radiation.)
- Hurting a user
- Healing a user
- Teleporting to a user
- The ability to see a user's vitals, status and steamID64
- Use of PermissionsManager plugin to edit Oxide user permissions on a user
- Use of Freeze plugin to freeze/unfreeze user position (Only for English language users, Freeze uses localized commands!!!)
- Use of Economics plugin to show player's current balance
- Use of DiscordMessages plugin to send a fancy message to Discord for each ban and kick
- Filtering through users via the "search" function (Case insensitive and selects both names and IDs that contain the text written in the input)

## Permissions

- **playeradministration.show** -- Required to be able to use the `/padmin` command and plugin
- **playeradministration.kick** -- Allows the user to kick any player
- **playeradministration.ban** -- Allows the user to ban and unban any player
- **playeradministration.kill** -- Allows the user to kill any player
- **playeradministration.clearinventory** -- Allows the user to clear any player's inventory
- **playeradministration.resetblueprint** -- Allows the user to reset any player's blueprints
- **playeradministration.resetmetabolism** -- Allows the user to reset any player's metabolism
- **playeradministration.recovermetabolism** -- Allows the user to give any player a healthy metabolic state
- **playeradministration.hurt** -- Allows the user to hurt any player
- **playeradministration.heal** -- Allows the user to heal any player
- **playeradministration.voicemute** -- Allows the user to mute the voice chat of any player
- **playeradministration.chatmute** -- Allows the user to mute the text chat of any player
- **playeradministration.perms** -- Allows the user to use the "Permissions" button for any player
- **playeradministration.freeze** -- Allows the user to freeze and unfreeze any player
- **playeradministration.teleport** -- Allows the user to teleport to any player

## Chat Commands

- **/padmin** -- Show the player administration menu ***(requires `playeradministration.show` permission)***

## Configuration

- **Use Permission System** -- When set to `false` the users with the `playeradministration.show` permission can use all actions

```json
{
  "Use Permission System": true,
  "Discord Webhook url for ban messages": "",
  "Discord Webhook url for kick messages": ""
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
  "Search Label Text": "Search:",
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
  "Economics Balance Label Format": "Balance: {0} coins",
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

  "Go Button Text": "Go",

  "Clear Inventory Button Text": "Clear Inventory",
  "Reset Blueprints Button Text": "Reset Blueprints",
  "Reset Metabolism Button Text": "Reset Metabolism",
  "Recover Metabolism Button Text": "Recover Metabolism",

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

  "Perms Button Text": "Permissions",
  "Perms Not Installed Button Text": "Perms Not Installed",
  "Freeze Button Text": "Freeze",
  "Freeze Not Installed Button Text": "Freeze Not Installed",
  "UnFreeze Button Text": "UnFreeze",

  "Voice Mute Button Text": "Mute Voice",
  "Voice Unmute Button Text": "Unmute Voice",
  "Chat Mute Button Text": "Mute Chat",
  "Chat Unmute Button Text": "Unmute Chat"
}
```
