# TODO

## INCOMING




## MUST - usable at work

### documentation comments

### readme

### github setup
- build and publish actions
- make repo public

### announce it
- blog article
- post in gitter channel


# switch at work
- adapt DeflateMessagePackSerializer
- remove the attributes from model
- remove the *direct* dependency on msgpack

## FUTURE

### get coverage as high as possible

### performance check BidirectionalMapping
- is it really faster than just a single dictionary with reverse lookup
- potentially in the Build() step, compile it to a better lookup structure
- abstract it behind interface


# Doc
- indexer properties are not used
differences to MessagePack
- types must be default-constructable and public
- properties must be public and writeable

recommendation: configure non-attributeless stuff before .Configure() call