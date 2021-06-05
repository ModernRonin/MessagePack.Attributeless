# TODO

## INCOMING




## MUST - usable at work

### Tests
- Write tests proving that all automatic config actions create sorted, ie guaranteed repeatable config
  - Autokey
  - AllSubTypesOf
  - GraphOf
- Tests for equivalence of different config methods using keytable 
- Check whether referencedtypes gets all subtypes or GraphOf passes assemblies to AllSubTypes
- test with type that has indexer properties

### Overrides
- [T] Allow to override formatter for type 
- [T] Allow null-formatter, doesn't even write out property key, is not used as subtype 
- Allow to ignore property, directly by expression, and via predicate 

### Keytable
- Must contain which native formatters
- Any override info 
- Should have a different, better name 

### add easy way to validate configuration in a unit-test

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


