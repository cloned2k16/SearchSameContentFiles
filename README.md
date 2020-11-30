# SearchSameContentFiles
inspired by an amazon test


The overall idea is to make a very scalable algorithm being able to process an huge quantity of very huge similar fies
reducing as much as possible file IO while not being able to access CRCs directly from disk (which otherwise would make things easier ) ...

so .. we collect files in a Set and don't really care about which kind .. ( this a sort of trash can / queue ) 
cause we focus on the really important issue here ..
having an huge ammount of quasi identical huge files while in fact different and with chances to make HASH collision anyway ...

we read from file in chunks ( sparse better (future) ) compare them byte by byte and calculate a digest of each chunk aside of keeping the state of 
changes detected so we can finally collect identical files sorted ( by insertion )
where same files have identical state and full comparison, while most of not identical file will get a much lower attention and get ignored as soon as possible




