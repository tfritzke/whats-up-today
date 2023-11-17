
**1.1.0**
+ Update to Net 8.0.

**1.0.4**
+ Update to Net 7.0.8.
+ Minor bug fixes.

**1.0.3**
* Core:
  * Add exception helper class Ensure.
  * Add string extensions.
* Data:
  * Add entity interfaces for AutoSave (see IAutoSaveEntity)
  * Add DbContext extensions for AutoSave, which automatically sets the Entity creation/modification dates and user for tracked entities based on AutoSave interfaces.
* Test:
  * Add test libraries for Core and Data.
  * Add unit tests for AutoSave extension.
 
**1.0.2**
* Core: Adds generic Entity types.
* Data: Adds generic services for Entity types.

**1.0.1**
* Thin library to test nuget publishing.

**1.0.0**
* Initial release supporting .net7.0.
* Shared under the Apache 2.0 license.
