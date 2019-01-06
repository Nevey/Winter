Deformable Surface v2.0

---

Requirements: DX11 & Shader Model 5.0 support

This asset allows you to create and manage the system with realtime volumetric surface deformation. You might already noticed similar feature in some AAA titles. Such feature will definitely make your game more dynamic and realistic looking. To achieve the best performance it uses true power of compute shaders along with DX11 hardware tessellation.

Read the included documentation for more info

---

E-mail me at Voodoo2211@gmail.com if you have any questions or suggestion

Also, check out more useful things for Unity here: 
https://www.assetstore.unity3d.com/#/publisher/979

---

Changelog:

> v2.0
New:
* Access to save/load the state of deformation
* New additional shader: Standard Cutout
* Quad patch tesselation
* Heightmap import
* New parameters: Normal Intensity, Max Height
* Custom shaders are now allowed
* All texture maps combined into one ARGB32 format
* Linear color space support
* Unity 2018.3 support
* Overall performance improvement

Fixes:
* Incorrect base mesh normals
* Deformation smoothing is not affected at top height
* Odd deformation and smoothing at top-right edges of base map
* Performance issue in painting mode
* Surface double-rendering issue caused by depth camera
* Various minor fixes

> v1.1.2
* Fixed bug where the whole surface has been deformed at scene start
* Fixed issue with incorrect lights in Unity 2017.2 and above

> v1.1.1
* Fixed issue with SetTexture failed and normal map generation for newly created surface

> v1.1
* Metallic Smoothness texture slot was removed. Alpha value of Albedo texture map is now used as a source for smoothness
* Inverted version of depth camera shader for Unity 5.5 was removed. Shader directive is now handle this
* Added maps quality setting which allows to save some VRAM almost without quality loss
* Editor side compute shader logic moved to separate .compute file

> v1.0
* Initial release