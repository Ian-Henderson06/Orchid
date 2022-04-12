In development game-based abstraction layer build on Riptide.

Current Features:
  - Networked ID system.
  - Networked entity spawning and basic state updates (position, rotation).
  - Server authoritative system.
  - Two way RPC calls => Client <---> Server.
  - Event system.
  - World state based updates (state of all networked objects on server without specific packet sending).
  - Synced tick system.

To Add:
 - Client controlled entities.
 - Assigning authority/requesting authority.
 - Client interpolation.
 - Client prediction & server reconcilliation.
 - More events.
