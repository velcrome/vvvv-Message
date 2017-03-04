Message Keep
============

One way to grasp a *Keep* is to take it as an archive of **Message**s. While a 'normal' **Message** only has a life cycle of 1 frame, a *Keep* can, well, 'keep' it for the next or any other number of vvvv's frames. This archive keeps also tight management about any change to all kept **Message**s, no matter where in your patch it happens, up AND down. 

> If this sounds all too arcane to you, let me tell you that this kind of stuff was necessary.
> Necessary to go about the weirdness, if you have multiple editing nodes in arbitrary evaluation order, which is ofc a valid vvvv proposition

VVVV veterans might be wondering about the lack of easily recognizable node names, like S+H, Queue, Buffer and the likes. Instead, this pack relays you onto the new concept of 'Keep'. This suite's been called "exotic" for this. During the years of development, it became clear, that **Message** needs a different breed of stateful nodes, that are more focussed on "joining" Messages, then an "replacing values", because that's what commonly happens: only a few *Fields* within your dataset will actually change during the same frame.

> Isolated changed *Field*s are commonly called **Diff** for the *Keep* nodes, and can be activated with Herr Inspektor.

The reason why the *Keep* nodes might feel exotic at first, is because they are high-level abstractions of patch patterns that occurred recently.

Hold
----

The `HoldKeep (Message)` node is a prime example. It unifies a known `S+H` with a `Change` on its input, because that's what you do in 80% of the use cases. Just hold on tight for the last one. 

Safe
----

The `SafeKeep (Message)` conveys even more complex scenarios, where patchers usually had to jump hoops to sift a correct index from a FrameDelay convolution. The `SafeKeep (Message)`. This is all done for you here, if a **Message** with a matching *Topic* is inputted into this *Keep*

Session
-------

`SessionKeep (Message)` is very similar to the Safe, but it allows **Message** matching not only for the *Topic*, but for any amount of matching *Field*s.
It comes in handy, if you compare device ids or web session ids, hence the name.

Config
------

This node is your shortcut node, allowing you to set initial values, and keep it. Provides the same convenience as an Edit node, but right where you first created it.
