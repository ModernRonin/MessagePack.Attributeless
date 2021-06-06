# TODO

## INCOMING




## MUST - usable at work

### Tests
- Write tests proving that all automatic config actions create sorted, ie guaranteed repeatable config
  - Autokey
  - AllSubTypesOf
  - GraphOf
- Tests for equivalence of different config methods using keytable 

### revisit all exceptions being thrown
- do they give good info
- test

### make all types sealed

### tidy up namespace
- move stuff that is not really relevant to users into sub-namespaces
- hide user-relevant types behind interfaces

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

### announce it
- blog article
- post in gitter channel



## FUTURE

### get coverage as high as possible

### performance check BidirectionalMapping
- is it really faster than just a single dictionary with reverse lookup
- potentially in the Build() step, compile it to a better lookup structure
- abstract it behind interface


# Doc
- indexer properties are not used