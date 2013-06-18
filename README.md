Introduction
============
https://github.com/velcrome/vvvv-Message

This pack defines a new Message type for VVVV which lets you pack dynamic Spreads of various Types into one single Object.

It comes with some handy helper nodes to work with it (Sift, Select, Getslice, etc.)

Furthermore it ships with plugins to create OSC-Data as well as recreate a Message from OSC. While this might help to handle OSC Messages with vvvv it also shows the aweful state of both the OSC specification and the vvvv implementation of it - it cannot handle colors, transforms, vectors, hell, not even double.
Therefore it ships with a custom json serialisation for all Message types, so you can send them easily over UDP or TCP/IP.

Also there is a proof of concept for binary serialisation. This is standard .Net serialisation, so it is slow and chunky. Not recommended for usage.

Marko Ritter (www.intolight.com)

Credits
=======
Thanks Elias. Thanks also to the other core VVVV developers and users.

Libraries
=======
Json.NET 5.0, utilizing the MIT License ( http://james.newtonking.com/projects/json-net.aspx )


Generics
=======
The Nodes

* Framedelay
* Serialize (dependend on DataContractAttribute)
* S+H (ICloneable)
* Select
* GetSlice

have been implemented in a Generic Way. Might be a good idea to add them to the vvvv sdk.

License
=======

This plugin is distributed under the MIT license