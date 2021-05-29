# TODO

## INCOMING




## MUST - usable at work
### size comparisons
- with attributed msgpack
- with contractless
- with typeless

### speed comparisons
- with attributed msgpack
- with contractless
- with typeless

### test on huge data-structure, lots of types and properties, work stuff, separate private repo

### automatically configure all implementations of interface

### documentation comments

### readme

### license

### github setup
- build and publish actions
- make repo public

### switch at work
if prototype solution works out
- adapt DeflateMessagePackSerializer
- remove the attributes from model
- remove the *direct* dependency on msgpack


## SHOULD
### announce it
- blog article
- post in gitter channel


## COULD

### performance check BidirectionalMapping
- is it really faster than just a single dictionary with reverse lookup
- potentially in the Build() step, compile it to a better lookup structure
- abstract it behind interface

## WOULD

