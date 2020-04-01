# redis-stream-vs-pubsub
Perf test for redis stream and pub/sub.

## Test method

* Redis server: localhost
* Message producer count: 1
* Message consumer count: 1
* Message size: ~1K
* VM: Standard F4 (4 vcpus, 8 GiB memory) in Azure
* OS: Linux (ubuntu 18.04)
* Netcore: 3.1
* StackExchange.Redis: 2.1.28
* Redis: 5.0.8

## Test result

Redis stream v.s. redis pub/sub: 55 v.s. 100.

```
ConsoleApp2 Information: 0 : Connect to redis: 127.0.0.1
ConsoleApp2 Information: 0 : Start test pub/sub.
ConsoleApp2 Information: 0 : Start pub/sub round 0
ConsoleApp2 Information: 0 : Start pub/sub round 1
ConsoleApp2 Information: 0 : Pub/sub 142385.15356259234 per second.
ConsoleApp2 Information: 0 : Pub/sub 147592.43246965727 per second.
ConsoleApp2 Information: 0 : Start pub/sub round 2
ConsoleApp2 Information: 0 : Pub/sub 148143.38200647198 per second.
ConsoleApp2 Information: 0 : Start test stream.
ConsoleApp2 Information: 0 : Start stream round 0
ConsoleApp2 Information: 0 : Stream 81465.05114331855 per second.
ConsoleApp2 Information: 0 : Start stream round 1
ConsoleApp2 Information: 0 : Stream 81359.59338233773 per second.
ConsoleApp2 Information: 0 : Start stream round 2
ConsoleApp2 Information: 0 : Stream 80354.10165490567 per second.
```
