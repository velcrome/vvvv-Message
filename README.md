[![Build status](https://ci.appveyor.com/api/projects/status/xupapctmj83we10a/branch/master?pendingText=Master%20Pending&failingText=Master%20Fail&passingText=Master%20OK&svg=true)](https://ci.appveyor.com/project/velcrome/vvvv-message-tem27/branch/master) [![Build status](https://ci.appveyor.com/api/projects/status/xupapctmj83we10a/branch/develop?pendingText=Develop%20Pending&failingText=Develop%20Fail&passingText=Develop%20OK&svg=true)](https://ci.appveyor.com/project/velcrome/vvvv-message-tem27/branch/develop)

This node pack defines a new **Message** data link and c# type. 
Its initial purpose was to help you retain data and performance control if your vvvv project turns bigger than expected, without adding redundant and confusing links. 
Still truthful to that initialisation, it now strives to be the best multipurpose data plumbing tool to ever enter your workflow.

**Message** can help to communicate between threads, applications or even other devices. 

Structure & Access
------------------
Think of **Message** as a collection of (binsizeable) Spreads of various Types (**bool, int, float, double, string, Transform, Color, Vector**s**, Time, Message**), all into one single object. It also provides access to the [dx11](https://www.github.com/mrvux/dx11-vvvv) types **Layer, Geometry, Texture2d, Buffer**. 

You can create these Message objects with fully spreadable `Create (Message Formular)` and access the individual Spreads with  `Split (Message Formular)`, `Read (Message)` or simply `Info (Message)`. 

It comes with a broad range of handy helper nodes to work with it:
* `Sift (Message)`, `Sort (Message Formular`and search the Messages you are interested in
* handle Message spreads with ease: `GetSlice (Message)`, `Select (Message)`, et al. at your service
* micromanage Messages' Fields: Prune, `Topic (Message)` and `RenameField (Message)`
* handle edge cases explicitly with `Change (Message)`and `Clone (Message)`

Furthermore it ships with convenience plugins for persisting Messages to disc or serializing over network
* Json: `AsString (Message Json)`, `FromString (Message Json)`
* MsgPack: `AsMsgPack (Message Raw)`, `FromMsgPack (Message Raw)`
* OSC: `AsOscBundle (Message Raw)`, `FromOsc (Message Raw)`, `AsOscMessage (Message Formular)`, `FromOscMessage (Message Formular)` and some easter eggs.

On Strictness
-------------
This pack is open to tinkering and rapid prototyping. The most important plugin nodes' pin layout can be edited to your needs with a simple GUI window. It shows you what's possible.

As your project grows, you should look into the more strict alternative, and switch your **Formular** Pin from _None_ to one *you defined*, with `Formular (Message)`, which will manage your **Formular** definition application wide and make it available to all nodes with the tag _Formular_

SharpMessage
------------
The essence of this pack, its core so to speak, is fully managed and can be used independently of any specific Plugin implementation. It is freely available on nuget under the name [SharpMessage](https://www.nuget.org/packages/SharpMessage/) and also allows direct usage in any other .Net 4.6 application.


Author(s)
=========

Marko Ritter (www.intolight.com)

Thanks
------
* Elias
* Elliot
* Björn
* microdee
* lecloneur
* tmp
* sebl

Dependencies
----
* [Json.NET](http://james.newtonking.com/projects/json-net.aspx) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
* [MsgPack](http://msgpack.org/index.html)  [![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
* [VVVV.Packs.Time](https://github.com/letmp/vvvv-Time) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
* [VVVV-sdk](https://github.com/vvvv/vvvv-sdk) [![License: LGPL v2](https://img.shields.io/badge/License-LGPL%20v2-blue.svg)](http://www.gnu.org/licenses/lgpl-2.0)

License
=======
This software is distributed with [![License: CC BY-NC-SA 4.0](https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-blue.svg)](http://creativecommons.org/licenses/by-nc-sa/4.0/)

Please note overriding, more liberal licensing terms in individual subdirectories, such as

* SharpMessage [![License: LGPL v3](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](http://www.gnu.org/licenses/lgpl-3.0)
* easteregg OSC Nodes [![License: LGPL v3](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](http://www.gnu.org/licenses/lgpl-3.0)

Comments
--------

This software distribution defaults to the [CC Attribution-NonCommercial-ShareAlike 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) license.
This choice highlights, that this pack is for the community, really and open for all kinds of tinkering, learning and free remixing. 
If you find it useful, you can [flattr](https://flattr.com/profile/intolight) us.

However, if you want to employ this pack commercially (or have any other reason why this license doesn't fit your need), contact us, the maintainers: <license@intolight.de>. Rates start at 42€, so don't hesitate.
