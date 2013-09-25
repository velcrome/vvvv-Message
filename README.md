Introduction
============

This node pack defines a new **Message** data link and c# type for VVVV. Its primary purpose is to help you retain data and performance control if your project turns bigger than expected, without adding redundant boiler code nodes. It also helps you to interface with other applications. 

Think of this **Message** as a collection of (binsizeable) Spreads of various Types (bool, int, float, double, string, Transform, Color, Vectors), all into one single object. You can interface these Message objects with fully spreadable `Message (Split)` and `Message (Join)`. The plugin nodes' pin layout can be configured easily with good Herr Inspektor. The `Message (Split)` is worth of special mention because it serves as an almighty receiver. Right where you need the data.

It comes with some handy helper nodes to work with it (`Sift`, `Select`, `Getslice`, etc.).

Furthermore it ships with plugins to `Sift (OSC)`, `Bundle (OSC)`, `UnBundle (OSC)` and create **Raw** with `AsOSC (Message)`. The idea is nonetheless to use `AsMessage (OSC)` to recreate a meaningful **Message** as quickly as you can. Because **Message** strives to overcome a lot of shortcomings OSC currently has.

Just to prove this it ships with a typed JSon serialization, so you can send any Message with serializable attributes easily over UDP or TCP/IP or save and load from file. Binary serialisation is available too. 

Marko Ritter (www.intolight.com)

Credits
=======
Thanks to the core VVVV developers, other contributors and testers.
Special thanks to Elias, Elliot and Björn.

Libraries
=======
[Json.NET 5.0](http://james.newtonking.com/projects/json-net.aspx), utilizing the MIT License

YOLO
=======
* The Nodes `Serialize` (dependend on [DataContractAttribute](http://msdn.microsoft.com/en-us/library/system.runtime.serialization.datacontractattribute(v=vs.100).aspx)), `S+H`, `Select`, `GetSlice` have been implemented in a Generic Way. Might be a good idea to add them to the vvvv sdk.
* **Message** can be used hierarchically.
* `Message (Split)` and `Message (Join)` can be reconfigured remotely with `TypeConfig (Message)`. Feedback for that one is highly appreciated.

License
=======

This plugin is distributed under the [MIT license](http://opensource.org/licenses/MIT)

Feel free to use and improve this in any way, and allow yourself to contribute too
