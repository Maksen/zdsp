using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// code changed from Unity3D gradient effect for Unity3D 5.2
/// REF : http://pastebin.com/dJabCfWn
/// </summary>
public class UI_FontGradient : UnityEngine.UI.BaseMeshEffect
{
    [SerializeField]
    private Color32 topColor = new Color(1, 0.7765F, 0, 1);
    //FFC600FF
    [SerializeField]
    private Color32 bottomColor = new Color(1, 1F, 0.7882F, 1);
    //FEFFC9FF

#if UNITY_5_2_0 || UNITY_5_2_1
	public override void ModifyMesh( Mesh mesh )
	{
		if( !IsActive() ) { return; }
		if( mesh == null || mesh.vertexCount <= 0 ) return;

		int count = mesh.vertexCount;

		float bottomY = mesh.vertices[0].y;
		float topY = mesh.vertices[0].y;

		for( int i = 1; i < count; i++ )
		{
			float y = mesh.vertices[i].y;
			if( y > topY )
			{
				topY = y;
			}
			else if( y < bottomY )
			{
				bottomY = y;
			}
		}

		float uiElementHeight = topY - bottomY;

		List<Color32> new_clr = new List<Color32>();
		for( int i = 0; i < count; i++ )
		{
			new_clr.Add( Color32.Lerp( bottomColor, topColor, (mesh.vertices[i].y - bottomY) / uiElementHeight ) );
		}
		mesh.SetColors( new_clr );
	}
#else
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0) { return; }

        List<UIVertex> stream = new List<UIVertex>();
        vh.GetUIVertexStream(stream);

        //UIVertex tv; vh.PopulateUIVertex( ref tv, 0 );

        float bottomY = stream[0].position.y;
        float topY = stream[0].position.y;

        for (int i = 1; i < vh.currentVertCount; i++)
        {
            float y = stream[i].position.y;
            if (y > topY)
            {
                topY = y;
            }
            else if (y < bottomY)
            {
                bottomY = y;
            }
        }

        float uiElementHeight = topY - bottomY;
        UIVertex uiv = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref uiv, i);
            uiv.color = Color32.Lerp(bottomColor, topColor, (uiv.position.y - bottomY) / uiElementHeight);
            vh.SetUIVertex(uiv, i);
        }
    }
#endif
}