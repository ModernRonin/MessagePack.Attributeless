# TODO

## INCOMING




## MUST - usable at work
### automatically configure all implementations of interface

### Allow overrides
- register all implementations of a type, then override specifics for one implementation (for example, other formatter)
- autokeyed type, but override for one property, eg ignore
- add optional Func<PropertyInfo, bool> predicate to AutoKeyed

### size comparisons
- with attributed msgpack
- with contractless
- with typeless

### speed comparisons
- with attributed msgpack
- with contractless
- with typeless

### test on huge data-structure, lots of types and properties, work stuff, separate private repo

### make all types sealed

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

### revisit all exceptions being thrown
- do they give good info
- test

### add easy way to validate configuration in a unit-test

### tidy up namespace
- move stuff that is not really relevant to users into sub-namespaces
- hide user-relevant types behind interfaces


## COULD

### get coverage as high as possible

### performance check BidirectionalMapping
- is it really faster than just a single dictionary with reverse lookup
- potentially in the Build() step, compile it to a better lookup structure
- abstract it behind interface

## WOULD

