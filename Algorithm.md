# Algorithm

1. Determine the dungeon size.

2. Determine the player starting point on the first row and the boss room location on the last row.

3. Find shortest path between the player spawn room and the boss room

4. Choose two rooms and create a fork in the main path linking the rooms. If the fork must touch another room contained into the main path then the fork will be made between the room it passes through and the staring room of the fork. Repeat this process based on the dungeon size and exploratory parameters.
    - Forks are not allowed to bypass locked doors.

5. Choose a room that meets this criteria: It must not be adjacent to more than one room along the x and y axis. Create two doors inside this room, one locked and the other unlocked. Create a path of rooms that starts on the unlocked door and loops back into the chosen room through the locked door (Which should be unlocked when the player finishes the room). Repeat this process based on the dungeon size and exploratory parameters.
    - Loops paths must contain an item, a key or a shop vendor on the last room.
    - A special strong enemy can also be on the last room, as long as the player can choose to initiate the fight and and the fight must yield special rewards.
    - Loop paths should not have many enemies.
    - Doors that initiate the loop should be marked if it’s wished to do so when generating the level. By default loop paths shall not be marked.
    - The locked door can also be a one way door (e.g. it’s on a higher ground that the player can’t get up to).

6. Choose a random room that has free any free adjacent space: Create a path that leads nowhere (Dead End path). Repeat this process based on the dungeon size and exploratory parameters.
    - This path should be short(shorter than the average loops and forks).
    - A special strong enemy can also be on the last room, as long as the player can choose to initiate the fight and and the fight must yield special rewards.
    - Dead end paths should not have many enemies.
    - Doors that initiate the dead end should be marked if it’s wished to do so when generating the level. By default dead end paths shall not be marked.

7. Breather rooms can contain shortcuts to earlier parts of the level, mainly to other breather rooms, bank rooms or heal rooms. Shortcuts can not contain combat, but they can contain puzzles. Shortcuts should try to be as small as possible and they may use forks.

8. Locked doors should block the player main path. Keys must be found inside loops or dead ends. 
    - Each key necessary to complete the dungeon must be able to be found before the door that it unlocks. If loops or dead ends have locked doors then their key can be found on later areas.
    - To prevent unreachable optional rooms the keys will be color coded and they must be found between the available area that the player can get to without it. Keys also can not be found behind optional locked doors (Locked doors inside loops or dead ends).
    - If there are not enough loops and dead ends to meet the criteria of unreachable optional rooms then keys can be added to the main path, although this is discouraged due to players going on a forks without the key.
    - Locked doors inside loops or dead ends must yield better rewards than normal forks or loops.

9. After 3 to 5 combat rooms or Trap rooms, puzzle and breather rooms should be put in place so that the player doesn’t burn out.

10. Sub-bosses should be evenly spaced out. Before each Sub-boss there should be a heal room or a breather room with a shortcut to a breather room. Every sub boss room will become a heal room once the sub-boss is defeated.
    - The main boss might follow a sub boss.
    - Sub-bosses should be spaced out evenly throughout the main path.
    - Boss rooms should be made obvious to the player and they may opt to not fight the boss once entering the arena.

11. FOEs are enemies that follow the player through the dungeon and are tougher than normal enemies. Defeating them yields great rewards, but it might cost a life.

12. Room types can be combined to create multi-use rooms (e.g. Trap rooms + combat rooms or Bank rooms + Heal rooms). Rooms can also be merged to create bigger rooms

13. Room Types:
    - Boss
    - Breather
    - Combat
    - Heal
    - Initial
    - Item
    - Puzzle
    - Trap
    - Vendor
