Introduction
============

This node pack defines a new **Message** data link and c# type. Its primary purpose is to help you retain data and performance control if your vvvv project turns bigger than expected, without adding redundant and confusing links. **Message** can help to establish communication between threads or even other applications. 

Structure & Access
------------------
Think of **Message** as a collection of (binsizeable) Spreads of various Types (**bool, int, float, double, string, Transform, Color, Vector**s**, Time, Message**), all into one single object. 

You can create these Message objects with fully spreadable `Create (Message.Formular)` and access the individual Spreads with  `Split (Message)`, `Read (Message)` or simply `Info (Message)`. 

It comes with some handy helper nodes to work with it (`Sift`, `Select`, `Getslice`, etc.). 
Furthermore it ships with convenience plugins for OSC, a self-made binary encoding (from microdee) and brief, but fully typed json serialisation. Use any for sharedmemory, networking or hd recording.

Plugin Layout
-------------
Many plugin nodes' pin layout can be chosen either from a customizable Formular (think of it like a definition of a certain Message type) or even configured dynamically with Herr Inspektor. 

Message Flow
------------
Once created, a message zips across your patched application in a compact and manageable struct, even though its gestalt is highly dynamic. This helps to report any change from some view in your application to your data-mongering model.

To outfit your model, Message can be held in `Safe (Message.Keep)`, `Hold (Message.Keep)`, `Cache (Message.Keep)`, all keeping track of any change to the well-kept Messages.

Core
----
The core is fully managed and can be used independently of any specific Plugin implementation. It is freely available on nuget under the name VVVV.Packs.Message but of course allows direct usage in any other .Net application.

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

Nuget
----
* [Json.NET 5.0](http://james.newtonking.com/projects/json-net.aspx), utilizing the MIT License
* [VVVV.Packs.Time](https://github.com/letmp/vvvv-Time), utilizing the MIT License
* [VVVV-sdk](https://github.com/vvvv/vvvv-sdk), LGPL

License
=======
This software is distributed with the [CC Attribution-NonCommercial-ShareAlike 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) license.
![CC 4.0BY NC SAt](http://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png)

Please note individual licensing terms in individual subdirectories, such as

* The Message Core ![LGPL 3.0](https://www.gnu.org/graphics/lgplv3-147x51.png)
* OSC Nodes ![LGPL 3.0](https://www.gnu.org/graphics/lgplv3-147x51.png)

Comments
--------

This software distribution defaults to the [CC Attribution-NonCommercial-ShareAlike 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) license.

This choice highlights, that this pack is for the community, really and open for all kinds of tinkering, learning and free remixing. 

If you want to employ it commercially or have any other reason why this license doesn't fit your need, write a quick email to the maintainer <license@intolight.de>. 
If you are earning or saving some money by using it, get in touch and let's find a deal. I believe hand-crafted commercial work is a beautiful part of our community, too. 

However, if your product contains parts of this framework protected by the default license and has an annual net revenue in excess of €100.000 (or any other currency, international exchange market rates apply) contact us immediately and please tell us more about the nature of your product so intolight can decide about individual licensing. 

If you find it useful, you can [flattr](https://flattr.com/profile/intolight) us and, if needed intolight provides [official billing](http://www.intolight.de/impressum), too.