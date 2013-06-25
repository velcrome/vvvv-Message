Introduction
============

This node pack defines a new <b>Message</b> data link and c# type for VVVV. Its primary purpose is to help you retain data and performance control if your project turns bigger than expected, without adding redundant boiler code nodes. It also helps you to interface with other applications. 

Think of this <b>Message</b> as a collection of (binsizeable) Spreads of various Types (bool, int, float, double, string, Transform, Color, Vectors), all into one single object. You can interface these Message objects with fully spreadable <code>Message (Split)</code> and <code>Message (Join)</code>. The plugin nodes' pin layout can be configured easily with good Herr Inspektor. The <code>Message (Split)</code> is worth of special mention because it serves as an almighty receiver. Right where you need the data.

It comes with some handy helper nodes to work with it (<code>Sift</code>, <code>Select</code>, <code>Getslice</code>, etc.).

Furthermore it ships with plugins to <code>Sift (OSC)</code>, <code>Bundle (OSC)</code>, <code>UnBundle (OSC)</code> and create <b>Raw</b> with <code>AsOSC (Message)</code>. The idea is nonetheless to use <code>AsMessage (OSC)</code> to recreate a meaningful <b>Message</b> as quickly as you can. Because <b>Message</b> strives to overcome a lot of shortcomings OSC currently has.

Just to prove this it ships with a typed JSon serialization, so you can send any Message with serializable attributes easily over UDP or TCP/IP or save and load from file. Binary serialisation is available too. 

Marko Ritter (www.intolight.com)

Credits
=======
Thanks to the core VVVV developers, other contributors and testers.
Special thanks to Elias, Elliot and Björn.

Libraries
=======
Json.NET 5.0, utilizing the MIT License ( http://james.newtonking.com/projects/json-net.aspx )

YOLO
=======
* The Nodes <code>Serialize</code> (dependend on DataContractAttribute), <code>S+H</code>, <code>Select</code>, <code>GetSlice</code> have been implemented in a Generic Way. Might be a good idea to add them to the vvvv sdk.
* <b>Message</b> can be used hierarchically.
* <code>Message (Split)</code> and <code>Message (Join)</code> can be reconfigured remotely. No warning yet.

License
=======

This plugin is distributed under the MIT license

Feel free to use and improve this in any way, and contribute too if possible
