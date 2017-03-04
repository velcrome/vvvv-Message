Formular
========

A **Formular** is basically a description a **Message**, in fact many of them can be created from a single **Formular**. It simply provides a basic set of named and typed *Field*s.

It comes in two flavors. The obvious one is in the `Formular (Message)`. But don't be fooled, this one only allows you to define Formulars application wide. For prototyping, you can design a custom Formular  to each relevant node! That's right, when you click yourself a pin layout in a layout window, you are actually defining an internal Formular for that very node.

**Formular** is still quite spartan. No fancy metadata like min/max or precise default management provided yet. However, it allows a very blunt form of inheritance by simply flattening out all parent **Formular**s and prior checking for conflicts.

Field Declaration
============

Declarations of *Fields* happen a lot with **Message**, because it defines the internal structure of the message. It is the same with boththe window panel for Nodes set to Formuular "None", and with the `Formular (Message)` inputs.

Think of **Message** as a collection *Fields*, which is basically a named (and binsizeable) Spread of any of the various Types following:

* *bool*, *int*
* *float*, *double*
* *string*
* *Vector2d*, * *Vector3d*, * *Vector4d*
* *Transform* 
* *Color* - with alpha 
* *Raw* - a.k.a. Stream 
* *Time* - from tmp's [Time](https://github.com/letmp/vvvv-Time)
* **Message** - yes, it is nested and opens the door for recursion

Use any one of those Type aliases as you would in c#, something like 

> string MyFirstWord

First declare the *Fields* Type, then declare its name. For names, CamelCase is usually a good choice.
A declaration like above will yield a *Field* of Type string with a single value. 

> Time[10] MyTenFirstDates

This, on the contrary will yield structure for a list of 10 localized timestamps. This helps when using the ubiquitous `Create (Message)`, `Edit (Message)` and `Split (Message)`, because their pins will not only have the *Field*'s name, but also a preset bin size. Leave the array index empty, if you you don't know the number of items in the *Field* yet, just like this:

> Color[] FavoriteColors
