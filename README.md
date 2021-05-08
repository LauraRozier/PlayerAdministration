*Player Administration* provides a simple-to-use GUI that helps admins moderate users.
## Features

- Banning/unbanning users
- Kicking users
- Killing users
- Muting/unmuting a player
- Clearing the user's inventory
- Resetting a user's blueprints
- Resetting a user's metabolism (Identical to how a respawn sets a random metabolism)
- Recover a user's metabolism (Gives the user a healthy metabolic state by filling their hunger, thirst, oxygen, and removing bleeding and radiation.)
- Hurting a user
- Healing a user
- Teleporting yourself to a user
- Teleporting a player to yourself
- Spectating a player
- The ability to see a user's vitals, status and steamID64
- Use of Economics plugin to show player's current balance
- Use of ServerRewards plugin to show player's current reward points
- Use of Freeze plugin to freeze/unfreeze user position (Only for English language users, Freeze uses localized commands!!!)
- Use of PermissionsManager plugin to edit Oxide user permissions on a user
- Use of DiscordMessages plugin to send a fancy message to Discord for each ban and kick
- Use of BetterChatMute plugin to mute players in an improved manner
- Use of Backpacks to view a players backpack
- Use of Inventory Viewer to view a players inventory
- Filtering through users via the "search" function (Case insensitive and selects both names and IDs that contain the text written in the input)

## Permissions

Hint: To easily add all protections use the RCON command: `oxide.grant {user <username> | group <group name>} playeradministration.protect.*`

- **playeradministration.access.show** -- Required to be able to use the `/padmin` command and plugin
- **playeradministration.access.kick** -- Allows the user to kick any player
- **playeradministration.access.ban** -- Allows the user to ban and unban any player
- **playeradministration.access.kill** -- Allows the user to kill any player
- **playeradministration.access.clearinventory** -- Allows the user to clear any player's inventory
- **playeradministration.access.resetblueprint** -- Allows the user to reset any player's blueprints
- **playeradministration.access.resetmetabolism** -- Allows the user to reset any player's metabolism
- **playeradministration.access.recovermetabolism** -- Allows the user to give any player a healthy metabolic state
- **playeradministration.access.hurt** -- Allows the user to hurt any player
- **playeradministration.access.heal** -- Allows the user to heal any player
- **playeradministration.access.mute** -- Allows the user to mute/unmute any player
- **playeradministration.access.perms** -- Allows the user to use the "Permissions" button for any player
- **playeradministration.access.allowfreeze** -- Allows the user to freeze and unfreeze any player
- **playeradministration.access.teleport** -- Allows the user to teleport to any player
- **playeradministration.access.spectate** -- Allows the user to spectate any player
- **playeradministration.access.detailedinfo** -- Allows the user to see more detailed player information
- **playeradministration.protect.ban** -- Protect the user against banning through the panel
- **playeradministration.protect.hurt** -- Protect the user against hurting through the panel
- **playeradministration.protect.kick** -- Protect the user against kicking through the panel
- **playeradministration.protect.kill** -- Protect the user against killing through the panel
- **playeradministration.protect.reset** -- Protect the user against stat/BP/inventory resetting/clearing through the panel

## Chat Commands

Binding keys and saving the keybinds:
 `bind p chat.say 0 /padmin`
or:
 `bind p "chat.say 0 /padmin"`
Then to save it:
 `writecfg`

- **/padmin** -- Show the player administration menu ***(requires `playeradministration.access.show` permission)***

## Console Commands

- **playeradministration.closeui** -- Close the player administration menu
- **playeradministration.switchui <UI Page Type>** -- Switch the UI to a different page (Check the code to see the types of UIPage) ***(requires `playeradministration.show` permission)***
- **playeradministration.kickuser <Player ID>** -- Kick a player ***(requires `playeradministration.access.kick` permission)***
- **playeradministration.banuser <Player ID>** -- Ban a player ***(requires `playeradministration.access.ban` permission)***
- **playeradministration.mainpagebanbyid** -- Ban a player ***(requires `playeradministration.access.ban` permission AND only works from the UI due to the text input field)***
- **playeradministration.unbanuser <Player ID>** -- Unban a player ***(requires `playeradministration.access.ban` permission)***
- **playeradministration.perms <Player ID>** -- Open the perms UI for a player ***(requires `playeradministration.access.perms` permission)***
- **playeradministration.vmuteuser <Player ID>** -- Mute voice for a player ***(requires `playeradministration.access.voicemute` permission)***
- **playeradministration.vunmuteuser <Player ID>** -- Unmute voice for a player ***(requires `playeradministration.access.voicemute` permission)***
- **playeradministration.cmuteuser <Player ID>** -- Mute chat for a player ***(requires `playeradministration.access.chatmute` permission)***
- **playeradministration.cunmuteuser <Player ID>** -- Unmute chat for a player ***(requires `playeradministration.access.chatmute` permission)***
- **playeradministration.freeze <Player ID>** -- Freeze a player ***(requires `playeradministration.access.allowfreeze` permission)***
- **playeradministration.unfreeze <Player ID>** -- Unfreeze a player ***(requires `playeradministration.access.allowfreeze` permission)***
- **playeradministration.clearuserinventory <Player ID>** -- Clear the inventory of a player ***(requires `playeradministration.access.clearinventory` permission)***
- **playeradministration.resetuserblueprints <Player ID>** -- Completely reset the BPs of a player ***(requires `playeradministration.access.resetblueprint` permission)***
- **playeradministration.resetusermetabolism <Player ID>** -- Reset the metabolism of a player to fresh spawn state ***(requires `playeradministration.access.resetmetabolism` permission)***
- **playeradministration.recoverusermetabolism <Player ID>** -- Recover the metabolism of a player to 100% ***(requires `playeradministration.access.recovermetabolism` permission)***
- **playeradministration.hurtuser <Player ID> <Amount>** -- Hurt a player for a certain amount ***(requires `playeradministration.access.hurt` permission)***
- **playeradministration.killuser <Player ID>** -- Kill a player ***(requires `playeradministration.access.kill` permission)***
- **playeradministration.healuser <Player ID> <Amount>** -- Heal a player for a certain amount ***(requires `playeradministration.access.heal` permission)***
- **playeradministration.tptouser <Player ID>** -- Teleport to a player ***(requires `playeradministration.access.teleport` permission)***
- **playeradministration.tpuser <Player ID>** -- Teleport a player to you ***(requires `playeradministration.access.teleport` permission)***
- **playeradministration.spectateuser <Player ID>** -- Spectate a player ***(requires `playeradministration.access.spectate` permission)*** **Note: This will kill your character by the game's design!**

## Configuration

- **Use Permission System** -- When set to `false` the users with the `playeradministration.access.show` permission can use all actions

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
  "Protection Active Text": "Unable to perform this action, protection is enabled for this user",
  "Dead Player Error Text": "Unable to perform this action, the target player is dead",

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
  "ServerRewards Points Label Format": "Reward points: {0}",
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

  "Unban Button Text": "Unban",
  "Ban Button Text": "Ban",
  "Kick Button Text": "Kick",
  "Reason Input Label Text": "Reason:",

  "Unmute Button Text": "Unmute",
  "Mute Button Text": "Mute",
  "Mute Button Text 15": "Mute 15 Min",
  "Mute Button Text 30": "Mute 30 Min",
  "Mute Button Text 60": "Mute 60 Min",

  "UnFreeze Button Text": "UnFreeze",
  "Freeze Button Text": "Freeze",
  "Freeze Not Installed Button Text": "Freeze Not Installed",

  "Clear Inventory Button Text": "Clear Inventory",
  "Reset Blueprints Button Text": "Reset Blueprints",
  "Reset Metabolism Button Text": "Reset Metabolism",
  "Recover Metabolism Button Text": "Recover Metabolism",

  "Teleport To Player Button Text": "Teleport To Player",
  "Teleport Player Button Text": "Teleport Player",
  "Spectate Player Button Text": "Spectate Player",

  "Perms Button Text": "Permissions",
  "Perms Not Installed Button Text": "Perms Not Installed",

  "Hurt 25 Button Text": "Hurt 25",
  "Hurt 50 Button Text": "Hurt 50",
  "Hurt 75 Button Text": "Hurt 75",
  "Hurt 100 Button Text": "Hurt 100",
  "Kill Button Text": "Kill",

  "Heal 25 Button Text": "Heal 25",
  "Heal 50 Button Text": "Heal 50",
  "Heal 75 Button Text": "Heal 75",
  "Heal 100 Button Text": "Heal 100",
  "Heal Wounds Button Text": "Heal Wounds"
}
```