using UnityEngine;
using UnityEngine.Networking;

public class PlayerBrush : NetworkBehaviour
{
    Renderer objRender;

    #region Initialization
    [Server]
    private void Start()
    {
        //Fetch the GameObject's Renderer component
        objRender = GetComponent<Renderer>();
        //Change the GameObject's Material Color to red
        objRender.material.color = Color.red;

        var data = PaintCanvas.GetAllTextureData();
        var zippeddata = data.Compress();

        RpcSendFullTexture(zippeddata);
    }

    [ClientRpc]
    private void RpcSendFullTexture(byte[] textureData)
    {
        PaintCanvas.SetAllTextureData(textureData.Decompress());
    }
    #endregion

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {

                //Get Coords of pixel we hit
                Vector2 pixelUV = hit.textureCoord;
                //Get reference to gameObject rendered
                Renderer rend = hit.transform.gameObject.GetComponent<Renderer>();
                //Get the gameObject texture
                Texture2D tex = rend.material.mainTexture as Texture2D;

                Color color = rend.material.color;

                float pixelUVX = pixelUV.x;
                float pixelUVY = pixelUV.y;

                int centerX = (int)Mathf.Round(pixelUVX * tex.width);
                int centerY = (int)Mathf.Round(pixelUVY * tex.height);

                int radius = sqrt(centerX** + centerY**) ;
                for (int j = 0; j <= radius; j++)
                {

                    for (int i = 0; i < 360; i++)
                    {

                        double angle = i * System.Math.PI / 180;
                        int x = (int)(j * System.Math.Cos(angle));
                        int y = (int)( j * System.Math.Sin(angle));

                        tex.SetPixel(x, y, ColorPicker.SelectedColor);
                    }
                }

                var pallet = hit.collider.GetComponent<PaintCanvas>();
                if (pallet != null)
                {
                    Debug.Log(hit.textureCoord);
                    Debug.Log(hit.point);

                    MeshCollider meshCollider = hit.collider as MeshCollider;

                    if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
                        return;

                    pixelUV.x *= tex.width;
                    pixelUV.y *= tex.height;

                    CmdBrushAreaWithColorOnServer(pixelUV, ColorPicker.SelectedColor, BrushSizeSlider.BrushSize);
                    BrushAreaWithColor(pixelUV, ColorPicker.SelectedColor, BrushSizeSlider.BrushSize);
                }

                tex.Apply();
            }
        }
    }

    [Command]
    private void CmdBrushAreaWithColorOnServer(Vector2 pixelUV, Color color, int size)
    {
        RpcBrushAreaWithColorOnClients(pixelUV, color, size);
        BrushAreaWithColor(pixelUV, color, size);
    }

    [ClientRpc]
    private void RpcBrushAreaWithColorOnClients(Vector2 pixelUV, Color color, int size)
    {
        BrushAreaWithColor(pixelUV, color, size);
    }

    private void BrushAreaWithColor(Vector2 pixelUV, Color color, int size)
    {
        for (int x = -size; x < size; x++)
        {
            for (int y = -size; y < size; y++)
            {
                PaintCanvas.Texture.SetPixel((int)pixelUV.x + x, (int)pixelUV.y + y, color);

            }
        }

        PaintCanvas.Texture.Apply();
    }
}
