The Message Nodes are built upon the MessageCore and supplies a suite of user-centered nodes.

Introduction
============


Think of **Message** as a collection of (binsizeable) Spreads of various Types 

* **bool**
* **int**
* **float**
* **double**
* **string**

* **Vector2d**
* **Vector3d**
* **Vector4d**

* **Transform** 
* **Color** - with alpha 
* **Raw** - a.k.a. Stream 
* **Time** - from tmp's [Time](https://github.com/letmp/vvvv-Time)

* **Message** - yes, it is recursive

Basic nodes such as `Create (Message)`, `Split (Message)`, `Edit (Message)` allow interfacing these vvvv link types easily. Right-Click any node to see it's defining Formular, and check which one's of them you need as pins.

When you don't have a formular selected, you can create a dynamic pin layout by simply double-clicking into the empty area of the Formular and editing any descriptor. 

Note, that all pins are accompanied by a hidden bin size. This means, that any field of a **Message** can be one or many items.

Also you'll see that all access to the fields of a message is "named"- including the vvvv pins. You can use `Read (Message)` and `Write (Message)` to that advantage.


Formular
========




Keep
====





Have you ever had too many links in your project? 


Maybe your project was even so cluttered in links, it became necessary to Zip and Cons stuff together, and pick it apart somewhere else. In this case it is hard to remember what's what, and hence even harder to maintain the project long term?



This happened to you, while you used S and R nodes? 