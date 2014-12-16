Introduction
============

This node pack defines a new **Message** data link and c# type for VVVV. Its primary purpose is to help you retain data and performance control if your project turns bigger than expected, without adding redundant and confusing links. **Message** also helps you to interface with other applications. 

A message zips across your application in a compact and manageable shape, even though its gestalt is highly dynamic. Its main purpose is to report any change from some view in your application to your data-mongering model. 

Think of **Message** as a collection of (binsizeable) Spreads of various Types (**bool, int, float, double, string, Transform, Color, Vector**s**, Time, Message**), all into one single object. You can create these Message objects with fully spreadable `Create (Message)` and access the data with  `Split (Message)`, `Read (Message)` or simply `Info (Message)`. 
Many plugin nodes' pin layout can be chosen from a Formular with a few clicks or even configured dynamically with Herr Inspektor. The `Message (Split)` is worth of special mention because it serves as an almighty receiver. Right where you need the data.

It comes with some handy helper nodes to work with it (`Sift`, `Select`, `Getslice`, etc.).

To outfit your model, Message can be held in `Safe (Message.Keep)`, `Hold (Message.Keep)`, `Cace (Message.Keep)` or a number of other Keeps. 

Furthermore it ships with farewell plugins for OSC, a self-made binary encoding (from microdee) and brief, but fully typed json serialisation. Use any for sharedmemory, networking or hd recording.

Marko Ritter (www.intolight.com)

Credits
=======
Thanks to 

* Elias
* Elliot
* Björn
* microdee
* lecloneur


Libraries
=======
[Json.NET 5.0](http://james.newtonking.com/projects/json-net.aspx), utilizing the MIT License

License
=======

This plugin is distributed under the [MIT license](http://opensource.org/licenses/MIT)

Feel free to use and improve this in any way, and allow yourself to contribute too
