[![Build status](https://ci.appveyor.com/api/projects/status/xupapctmj83we10a/branch/master?pendingText=Master%20Pending&failingText=Master%20Fail&passingText=Master%20OK&svg=true)](https://ci.appveyor.com/project/velcrome/vvvv-message-tem27/branch/master) [![Build status](https://ci.appveyor.com/api/projects/status/xupapctmj83we10a/branch/develop?pendingText=Develop%20Pending&failingText=Develop%20Fail&passingText=Develop%20OK&svg=true)](https://ci.appveyor.com/project/velcrome/vvvv-message-tem27/branch/develop)

This node pack defines a new **Message** data link and c# type. Its primary purpose is to help you retain data and performance control if your vvvv project turns bigger than expected, without adding redundant and confusing links. **Message** can help to establish communication between threads or even other applications. 

Structure & Access
------------------
Think of **Message** as a collection of (binsizeable) Spreads of various Types (**bool, int, float, double, string, Transform, Color, Vector**s**, Time, Message**), all into one single object. 

You can create these Message objects with fully spreadable `Create (Message Formular)` and access the individual Spreads with  `Split (Message Formular)`, `Read (Message)` or simply `Info (Message)`. 

It comes with a broad range of handy helper nodes to work with it:
* filter and search certain Messages, handle Message spreads with ease
* inspect and manipulate individual Messages

Furthermore it ships with convenience plugins for persisting Messages to disc or serializing over network
* Json
* MsgPack
* OSC

Formular
--------
Many plugin nodes' pin layout can be edited to your needs with a simple GUI window.
As a more strict alternative, you can select a **Formular**, which you can manage application wide.

Core
----
The core is fully managed and can be used independently of any specific Plugin implementation. It is freely available on nuget under the name [SharpMessage](https://www.nuget.org/packages/SharpMessage/) and also allows direct usage in any other .Net application.

Credits
=======

Author
------
Marko Ritter (www.intolight.com)

Thanks
------
* Elias
* Elliot
* Björn
* microdee
* lecloneur

Dependencies
----
* [Json.NET](http://james.newtonking.com/projects/json-net.aspx) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
* [MsgPack](http://msgpack.org/index.html)  [![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
* [VVVV.Packs.Time](https://github.com/letmp/vvvv-Time) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
* [VVVV-sdk](https://github.com/vvvv/vvvv-sdk) [![License: LGPL v2](https://img.shields.io/badge/License-LGPL%20v2-blue.svg)](http://www.gnu.org/licenses/lgpl-2.0)

License
=======
This software is distributed with [![License: CC BY-NC-SA 4.0](https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-blue.svg)](http://creativecommons.org/licenses/by-nc-sa/4.0/)

Please note individual licensing terms in individual subdirectories, such as

* The Message Core [![License: LGPL v3](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](http://www.gnu.org/licenses/lgpl-3.0)
* OSC Nodes [![License: LGPL v3](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](http://www.gnu.org/licenses/lgpl-3.0)

Comments
--------

This software distribution defaults to the [CC Attribution-NonCommercial-ShareAlike 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) license.
This choice highlights, that this pack is for the community, really and open for all kinds of tinkering, learning and free remixing. 
If you find it useful, you can [flattr](https://flattr.com/profile/intolight) us.

However, if you want to employ it commercially (or have any other reason why this license doesn't fit your need), write a quick email to the maintainer <license@intolight.de>. 
