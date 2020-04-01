# redis-stream-vs-pubsub
Perf test for redis stream and pub/sub.

## Test method

* Redis server: localhost
* Message producer count: 1
* Message consumer count: 1
* Message size: ~1K
* VM: Standard F4 (4 vcpus, 8 GiB memory) in Azure
* OS: Linux (ubuntu 18.04)
* Release: netcoreapp 3.1 (self-contained, linux-x64)
* StackExchange.Redis: 2.1.28
* Redis: 5.0.8 ([install](https://www.digitalocean.com/community/tutorials/how-to-install-and-secure-redis-on-ubuntu-18-04), no ssl, no auth)

## Test result

Redis stream v.s. redis pub/sub: 55~60 v.s. 100.

### Localhost

```
perf Information: 0 : Connect to redis: 127.0.0.1
perf Information: 0 : Start test pub/sub.
perf Information: 0 : Start pub/sub round 0
perf Information: 0 : Start pub/sub round 1
perf Information: 0 : Pub/sub 142385.15356259234 per second.
perf Information: 0 : Pub/sub 147592.43246965727 per second.
perf Information: 0 : Start pub/sub round 2
perf Information: 0 : Pub/sub 148143.38200647198 per second.
perf Information: 0 : Start test stream.
perf Information: 0 : Start stream round 0
perf Information: 0 : Stream 81465.05114331855 per second.
perf Information: 0 : Start stream round 1
perf Information: 0 : Stream 81359.59338233773 per second.
perf Information: 0 : Start stream round 2
perf Information: 0 : Stream 80354.10165490567 per second.
```

### VNet in Azure
```
perf Information: 0 : Connect to redis: 172.16.14.5
perf Information: 0 : Start test pub/sub.
perf Information: 0 : Start pub/sub round 0
perf Information: 0 : Start pub/sub round 1
perf Information: 0 : Pub/sub 134699.34623138545 per second.
perf Information: 0 : Start pub/sub round 2
perf Information: 0 : Pub/sub 141679.16530052066 per second.
perf Information: 0 : Pub/sub 145454.5934465586 per second.
perf Information: 0 : Start test stream.
perf Information: 0 : Start stream round 0
perf Information: 0 : Start stream round 1
perf Information: 0 : Stream 87681.61781176 per second.
perf Information: 0 : Start stream round 2
perf Information: 0 : Stream 86132.11837850104 per second.
perf Information: 0 : Stream 87004.45916525187 per second.
```
