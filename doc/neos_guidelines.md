# Is Modding Neos Allowed?

**In short: maybe?**  
Now, for the long answer:

This document is not an official source of information, and should not be treated as such. This is my own interpretation of the Neos EULA and guidelines in relation to modding.

The official sources on what is allowed or disallowed are the following:
- [Neos EULA][eula]
- [Neos Usage Guidelines][guidelines]

Unofficial, but still useful:
- [Plugins Wiki Page][plugin]

## Relevant EULA Excerpt

> You are not permitted to:
> 
> - Edit, alter, modify, adapt, translate or otherwise change the whole or any part of the Software nor permit the whole or any part of the Software to be combined with or become incorporated in any other software, nor decompile, disassemble or reverse engineer the Software or attempt to do any such things

## Relevant Guidelines Excerpt

> 1. Modification and/or reverse engineering of the Neos Application is against the Neos EULA
> 2. There is an early plugin system which may be used to develop in the Neos Environment for the purposes of experimentation or enhancement. Please see #plugin-development channel on the [Neos Discord](https://discord.com/invite/StcdNe2w) for more information.

<!--
## Relevant [Privacy Policy] Excerpt
> ### Community Content
> 
> Our Service contains community created content not created, owned or directly curated by us. If you open a third party world, creation or otherwise access third party content trough our Service, we strongly advise you to review the Privacy Policy of such world, creation or content.
> 
> We have no control over and assume no responsibility for the content, privacy policies or practices of any third party creators, publishers or services.
-->

## The Takeaway
- The extent to which the guidelines protect [plugins] is not well defined. The EULA and guidelines are in conflict over whether plugins are allowed. Unlike the EULA, the guidelines are not a legal document, but they are what the moderation team enforce.
- Use common sense.
  - A plugin/mod that violates [guidelines] (e.g. something designed for asset theft or harassment) is clearly not okay, and will inevitably result in moderation action.
  - A plugin/mod that makes a minor quality of life tweak is unlikely to result in moderation action.
- NeosModLoader does not modify the Neos application, but one part of it may be considered disassembling.
- Mods require case-by-case consideration
  - Using Unity APIs is clearly allowed.
  - Using public Neos APIs is probably allowed.
  - Using private Neos APIs is a gray zone.
  - [Patching](https://harmony.pardeike.net/articles/intro.html#altering-functionality-patching) Neos methods in-memory is questionable.
  - [Transpiling](https://harmony.pardeike.net/articles/patching-transpiler.html) Neos methods is even more questionable, as it may be considered disassembling.
  - Altering game files on disk is not allowed.
  - Mods are extensions of the NeosModLoader plugin and therefore subject to the same protections the guidelines grant plugins.

# Going Into Detail: How NeosModLoader Interfaces with Neos
NeosModLoader only interfaces with Neos in two places: the hook and the compatibility hash.

## The Hook
The hook is the point where Neos "hooks" into NeosModLoader, allowing it to begin execution.

Typically, [plugins] use [components] to execute custom code. This is limiting as they can only begin execution once a world loads. Typically this involves putting a plugin's component into your local home.

The NeosModLoader plugin uses a different mechanism. Instead of using a component, it uses a connector. Neos loads connectors during its initial setup, at which point they can begin execution. As this system is independent of world loading it is more reliable. The connector implementation is in [ExecutionHook.cs](../NeosModLoader/ExecutionHook.cs).

This connector-based hook does not modify the Neos application, and only uses public Neos APIs.

## The Compatibility Hash
[Neos plugins][plugin], when loaded, alter your client's compatibility hash from what a vanilla client would have. You cannot join a session unless your compatibility hash is an exact match with the host.

Plugins are intended to add new features such as [components] and [LogiX nodes][logix] to the [data model], and altering the data model breaks multiplayer compatibility. This is why the compatibility hash exists, and why plugins alter it.

NeosModLoader does not change the data model, therefore it is 100% network compatible with vanilla Neos. Furthermore, mods cannot alter the [data model] because NeosModLoader does not perform important component post-processing.

In order to make a client using NeosModLoader compatible with vanilla Neos the compatibility hash must be restored to its default value. [NeosVersionReset.cs](../NeosModLoader/NeosVersionReset.cs) does the following:

1. Finds the original protocol version number by scanning FrooxEngine for a particular integer. This involves interpreting the IL bytes as instructions, which could be considered disassembling.
2. Calculates the default compatibility hash using the integer we just found.
3. Restores version and compatibility hash fields to their original values via [reflection](https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/reflection). This does not modify Neos code, but it does change the values of fields that are not intended to be changed.

# Is Modding Good for Neos?
> Your scientists were so preoccupied with whether or not they could, they didn’t stop to think if they should.  
> — Dr. Ian Malcolm

We'll take a break from the facts and talk opinions for a moment.

My opinion is yes, mods are healthy for the game. You might have deduced this from the fact that I just created a mod loader. But let's look at the pros and cons, and I'll try to be impartial.

## The Pros
- Tomorrow's quality of life improvements, today. Sometimes the Neos team needs to direct their efforts elsewhere. Allowing the community to make temporary fixes pending a permanent solution is an easy win. Example: [MotionBlurDisable](https://github.com/zkxs/MotionBlurDisable), which won't be implemented in Neos until a full Settings redesign.
- Mods let power users opt in to warranty-voiding tools. Sometimes a desired feature has a very niche audience and risks breaking in the future. But at the same time it can be very useful in the present. Example: [ExportNeosToJson](https://github.com/zkxs/ExportNeosToJson).
- The classic "if you outlaw guns only outlaws will have guns" argument. <!-- Example: [MyHostMyRules](https://github.com/zkxs/MyHostMyRules). -->
<!-- - Potential to improve the game. If the planets align, what was once a mod could be integrated into the game proper. But let's be honest, it's going to be easier for Froox to build the feature himself than porting some random garbage code in from a mod... -->
<!-- - Possible source of new developers. If you're hiring someone to program for Neos... why not hire someone who's *already* programming for Neos? -->

## The Cons
- Risk of abuse. The more control users have over the game the more damage they can potentially do.
- Risk of breakage. Users *should* be aware that mods void the warranty, but some people might complain in the wrong channels when mods break. And if things go very wrong there's potential for a mod to break an unmodified user over the network.
- Misdirection of effort. If mods create a problem the Neos team has to step in and solve that's less time spent improving the game.
<!-- - IP ramifications. Modding is a slippery slope to decompiling when the complete lack of documentation leaves you in the dark. But the same can be said for regular plugins. -->

# Afterword
The [EULA] and [guidelines] are subject to change. This document was last updated on **2021-08-16**, and may be out of date.

[eula]: https://store.steampowered.com/eula/740250_eula_0
[guidelines]: https://docs.google.com/document/d/1mqdbIvbj1b2LeFhNzfAASeTpRZk6vmbXISYLdTXTVR4/edit
[privacy policy]: https://wiki.neos.com/Neos_Wiki:Privacy_policy
[plugin]: https://wiki.neos.com/Plugins
[plugins]: https://wiki.neos.com/Plugins
[component]: https://wiki.neos.com/Component
[components]: https://wiki.neos.com/Component
[logix]: https://wiki.neos.com/LogiX
[data model]: https://wiki.neos.com/Core_Concepts#Data_Model
