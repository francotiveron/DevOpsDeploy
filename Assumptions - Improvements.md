# Release Retention Coding Exercise

## Assumptions

### Orphan records
Some records in the sample data refer to non existing parent records (e.g. releases with non existing project, deployments with non existing environment).
The assumption here has been that those record need to be ignored, logging the event.

### Input Format
The component works at Domain Object instance level. Ingesting raw input (text, json, ...) is an user application concern


## Improvements

### Orphan records
Construct the missing records with default names and the referenced id and include the orphaned records in the result calculation.

### Recent versions
Currently Release versions and creation date are ignored. Even though there aren't any deployments for those, it is likely they would be used soon.
The tool could retain releases with versions created after the most recent deployment for the associated project in all environments.

### Input Format
The component could accept raw input (as text, json, byte array, filepath), validate the input format and then convert to Domain Object instances