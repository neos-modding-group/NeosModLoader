# Problem Solving Techniques

Neos has many different ways to solve a problem: Components, Logix, HTTP connections to an external server, refhacking, plugins, and now mods. Some of these methods are supported, some aren't. We'll do a quick rundown of the methods and where they're applicable.

## Components and Logix

If you can solve a problem with Components and/or Logix that's probably the approach you should take, as the more advanced techniques are likely to be overkill.

## HTTP Connections

Neos provides Logix nodes to communicate with an external server via HTTP GET, POST, and websockets. This is great for:

- Heavy data processing that LogiX isn't well suited for
- Advanced data persistence (for simple things consider using Cloud Variables)
- Connecting Logix across multiple sessions

## RefHacking

Refhacking is a method that at considerable performance cost can get you an extremely sketchy but working component access. Refhacking is not supported and will break in the future, but sometimes it is the only way to do certain things without waiting for real component access.

My personal advice is to just put your component access ideas into a todo list and do them once we have real support. It's not fun when your creations break.

## Plugins

Plugins let you add new components and Logix nodes, but at the cost of breaking multiplayer compatibility. If you like multiplayer, they aren't going to give you much quality-of-life because you'll be forced into singleplayer to use them. Plugins are great for automating menial tasks that you do very infrequently, for example monopacking is nightmarish to do by hand but can be done with a single button via a plugin.

## Mods

Mods do **not** let you add new components and Logix nodes, but they do work in multiplayer. They are limited in what they can do without breaking multiplayer compatibility. You can imagine them as a "controlled desync". They are well-suited for minor quality-of-life tweaks, for example preventing your client from rendering motion blur. Making a larger feature with a mod isn't a great option, as you cannot rely on other clients also having the mod.
