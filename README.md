# Deferred Engine For Monogame

by https://twitter.com/kosmonautgames

Feedback -> http://community.monogame.net/t/deferred-engine-playground-download/8180   
         -> https://kosmonautblog.wordpress.com/
         
![Alt text](http://i.imgur.com/ucSrI29.png "soft shadows")


Features:
- an easy to use viewer, with lots of GUI options
- G-buffer creation with support for physically based materials ( albedo, normal, roughness, metallic, mask)
- Cook-Torrance specular shading and Oren-Nayar diffuse shading for point lights
- light and mesh frustum culling
- deferred point lights, directional lights and environment mapping
- soft shadows
- dynamically updating point light shadows depending on scene changes
- temporal anti-aliasing
- HDR Bloom
- screen space ambient occlusion (HBAO)
- screen sapce reflections
- linear HDR pipeline.
- EXPERIMENTAL: screen space emissive materials (not updated to work right now)


Controls:
- " ^ " / the key above TAB : debug console with suggestions (tab to autocomplete)
- Space: Go into editor mode 
  - R / T: Change transformation gizmos between translation and rotation
  - Del : Delete object
  - Insert : Copy object
- WASD : move the camera
- right mouse drag : rotate the camera
- F1 : Cycle through render targets (albedo, normals, depth etc.)


How to manipulate the scene
- See the Main / MainLogic.cs for details. Manipulate and add scene objects in Initialize() and Update();

Notes:

This solution provides a basic 3d deferred rendering engine implemented in Monogame. You will need to have Monogame (3.6.0187 or newer) installed and Visual Studio 2013+ to compile.

This is not intended to be an engine used for custom programs / games, but rather a playground which makes it easy to understand and implement custom shaders.

The sample scene contains the Sponza Atrium (from Crytek), the Stanford Dragon (http://www.cc.gatech.edu/projects/large_models/) and Daft Punk Helmets (by Anders Lejczak - http://www.colacola.se/expo_daft.htm)

