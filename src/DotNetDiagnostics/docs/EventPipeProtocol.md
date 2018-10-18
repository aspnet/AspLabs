# Event Pipe Protocol Specification

The Event Pipe Protocol is the protocol used between a monitored .NET application (called "Application") and a tracing tool (called "Monitor") to enable the Monitor to enable and disable Event Sources in the application, and to enable the Application to signal events, when they occur.

## Goals / Non-Goals

The goals of this protocol are:

1. To represent the set of operations that can be performed in a monitored .NET application and the set of notifications the application can provide
2. To be time-efficient to write and space-efficient in the Application's memory in order to have a minimal impact on the execution of the application.

Some non-goals of this protocol are:

1. To be time-efficient or space-efficient for the Monitor, or to be space-efficient on the Network. The highest priority is application performance. Space-efficient network traffic is desirable only insofar as it makes the Application logic time/space-efficient.

To summarize: The protocol should be trivial to write, but can be complicated to read. It should require no buffering to write but can require buffering when reading. The goal is to shift as much of the burden to the Monitor so that the Application can quickly blit the data out to the socket and move on.

## Ideas

Just a brain dump of some ideas:

* Binary format
* Be careful with length prefixes. If we require a length prefix over the whole message, the message has to be buffered before it can be flushed out to the socket. Better would be to length-prefix smaller chunks.
* In fact, it may be desirable to use delimiters and escaping instead. Delimiters generally mean the receiver has to buffer more, but that's OK here because the Monitor has less pressure to be time-/space-efficient.
* Unlike most network protocols, we should probably encode strings in UTF-16 since that makes it easier to blit the native .NET string type out to the network.
* Fixed-length integers are probably better than variable-length integers.