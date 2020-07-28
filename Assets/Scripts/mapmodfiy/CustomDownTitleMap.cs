using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomDownTitleMap : MonoBehaviour
{
    private OnlineMaps map;


    private void Start()
    {
        map = OnlineMaps.instance;

        // Subscribe to the tile download event.
        OnlineMapsTileManager.OnStartDownloadTile += OnStartDownloadTile;
       
    }

    private void OnStartDownloadTile(OnlineMapsTile tile)
    {
        // 参考 =>  OnlineMapsTileManager.StartDownloadTile()
        // 参考 => OnlineMapsRasterTile.LoadTileFromWWW()
        StartCoroutine(HttpGetTile(tile));
    }

    IEnumerator HttpGetTile(OnlineMapsTile tile)
    {
       // Debug.Log(tile.url);
        tile.status = OnlineMapsTileStatus.loading;
        
        string url = $"http://127.0.0.1:8000/roadmap?{tile.zoom}{tile.x}{tile.y}";
       
        UnityWebRequest request = UnityWebRequest.Get(url);
        
        yield return request.SendWebRequest();
      

        byte[] data = null;
        if (request.responseCode == 200)
        {
          data  = request.downloadHandler.data;
        }

        //不存在title
        if (data==null)
        {
            tile.status = OnlineMapsTileStatus.error;

        }
        else
        {
            // Note: create a texture only when you are sure that the tile exists.
            // Otherwise, you will get a memory leak.
            Texture2D tileTexture = new Texture2D(256, 256, TextureFormat.RGBA32,true);
            
                
            tileTexture.filterMode = FilterMode.Bilinear;
            tileTexture.ClearRequestedMipmapLevel();
            tileTexture.requestedMipmapLevel = 0;
            // Here your code to load tile texture from any source.
            tileTexture.LoadImage(data);

            // tileTexture.mipMapBias = -1;
            // Note: If the tile will load asynchronously, set
            // tile.status = OnlineMapsTileStatus.loading;
            // Otherwise, the map will try to load the tile again and again.

            // Apply your texture in the buffer and redraws the map.
            if (map.control.resultIsTexture)
            {

                // Apply tile texture
                (tile as OnlineMapsRasterTile).ApplyTexture(tileTexture as Texture2D);

                // Send tile to buffer
                map.buffer.ApplyTile(tile);

                // Destroy the texture, because it is no longer needed.
                OnlineMapsUtils.Destroy(tileTexture);
            }
            else
            {
                // Send tile texture
                tile.texture = tileTexture;

                // Change tile status
                tile.status = OnlineMapsTileStatus.loaded;
            }
            tile.MarkLoaded();

            //if (tile.status!=OnlineMapsTileStatus.disposed&&tile.status!=OnlineMapsTileStatus.error)
            //{
            //    if (tile.www==null)
            //    {
            //        tile.www = new OnlineMapsWWW(tile.url);
                   
            //    }
                
            //    OnlineMapsTile.OnTileDownloaded(tile);
            //}

        }


        // Note: If the tile does not exist or an error occurred, set
        // tile.status = OnlineMapsTileStatus.error;
        // Otherwise, the map will try to load the tile again and again.

        // Redraw map (using best redraw type)
        map.Redraw();
        
    }
}