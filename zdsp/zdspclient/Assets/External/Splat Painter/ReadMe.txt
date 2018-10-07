------------------
Splat Painter 1.11
------------------

A simple tool for editing Splatmap by painting on meshes.

-------------------
Video Demonstration
-------------------

https://www.youtube.com/watch?v=yyGJqehTuwM

-----------
Quick start
-----------

1- Setting Material

	1- Create a Material (Assets->Create->Material)
	2- Set the Shader to "Splatmap/Splatmap4Diff".
	3- Create a Splatmap (Assets->Create->Splatmap->Red 512x512)
	4- Drag & Drop the Splatmap Texture to the correct slot in the Material.
	5- Drag & Drop your different Albedo Textures in the Material.

2- Setting Mesh

	1- Drag & Drop the Material on a Mesh in the scene.
	2- Add a MeshCollider to the Mesh (can be removed after painting).

3- Painting

	1- Open the Splat Painter Window (Window->Splat Painter)
	2- Select your Mesh in the scene.
	3- Set your brush properties (Shape, Size and Opacity) in the Splat Painter Window.
	4- Select one of your albedo texture in the Splat Painter Window.
	5- Click on Start Drawing.
	6- Use left mouse button to paint your mesh.
	7- When finished, click on Stop Drawing.
	