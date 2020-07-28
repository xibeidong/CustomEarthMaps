/*     INFINITY CODE 2013-2019      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if !UNITY_WEBGL
using System.Threading;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// The main class. With it you can control the map.
/// </summary>
[AddComponentMenu("Infinity Code/Online Maps/Online Maps")]
[Serializable]
public class OnlineMaps : MonoBehaviour, ISerializationCallbackReceiver, IOnlineMapsSavableComponent
{
#region Variables
    /// <summary>
    /// The current version of Online Maps
    /// </summary>
    public const string version = "3.5.0.1";

    /// <summary>
    /// The minimum zoom level
    /// </summary>
    public const int MINZOOM = 1;

    /// <summary>
    /// The maximum zoom level
    /// </summary>
    public const int MAXZOOM = 20;

    #region Actions

    /// <summary>
    /// Event caused when the user change map position.
    /// </summary>
    public Action OnChangePosition;

    /// <summary>
    /// Event caused when the user change map zoom.
    /// </summary>
    public Action OnChangeZoom;

    /// <summary>
    /// Event caused at the end of OnGUI method
    /// </summary>
    public Action OnGUIAfter;

    /// <summary>
    /// Event caused at the beginning of OnGUI method
    /// </summary>
    public Action OnGUIBefore;

    /// <summary>
    /// Intercepts getting marker by the screen coordinates.
    /// </summary>
    public Func<Vector2, OnlineMapsMarker> OnGetMarkerFromScreen;

    /// <summary>
    /// The event is invoked at the end LateUpdate.
    /// </summary>
    public Action OnLateUpdateAfter;

    /// <summary>
    /// The event is called at the start LateUpdate.
    /// </summary>
    public Action OnLateUpdateBefore;

    /// <summary>
    /// Event which is called after the redrawing of the map.
    /// </summary>
    public Action OnMapUpdated;

    /// <summary>
    /// The event occurs after the addition of the marker.
    /// </summary>
    public Action<OnlineMapsMarker> OnMarkerAdded;

    /// <summary>
    /// Event is called before Update.
    /// </summary>
    public Action OnUpdateBefore;

    /// <summary>
    /// Event is called after Update.
    /// </summary>
    public Action OnUpdateLate;

    #endregion

    #region Static Fields

    public static bool isPlaying = false;

    /// <summary>
    /// Specifies whether the user interacts with the map.
    /// </summary>
    public static bool isUserControl = false;

    private static OnlineMaps _instance;

    #endregion

    #region Public Fields

    /// <summary>
    /// Allows drawing of map.\n
    /// <strong>
    /// Important: The interaction with the map, add or remove markers and drawing elements, automatically allowed to redraw the map.\n
    /// Use lockRedraw, to prohibit the redrawing of the map.
    /// </strong>
    /// </summary>
    public bool allowRedraw;

    /// <summary>
    /// URL of custom provider.\n
    /// Support tokens:\n
    /// {x} - tile x\n
    /// {y} - tile y\n
    /// {zoom} - zoom level\n
    /// {quad} - uniquely identifies a single tile at a particular level of detail.
    /// </summary>
    public string customProviderURL = "http://localhost/{zoom}/{y}/{x}";

    /// <summary>
    /// URL of custom traffic provider.\n
    /// Support tokens:\n
    /// {x} - tile x\n
    /// {y} - tile y\n
    /// {zoom} - zoom level\n
    /// {quad} - uniquely identifies a single tile at a particular level of detail.
    /// </summary>
    public string customTrafficProviderURL = "http://localhost/{zoom}/{y}/{x}";

    /// <summary>
    /// Texture displayed until the tile is not loaded.
    /// </summary>
    public Texture2D defaultTileTexture;

    /// <summary>
    /// Specifies whether to dispatch the event.
    /// </summary>
    public bool dispatchEvents = true;

    /// <summary>
    /// Color, which is used until the tile is not loaded, unless specified field defaultTileTexture.
    /// </summary>
    public Color emptyColor = Color.gray;

    /// <summary>
    /// Map height in pixels.
    /// </summary>
    public int height = 1024;

    /// <summary>
    /// Specifies whether to display the labels on the map.
    /// </summary>
    public bool labels = true;

    /// <summary>
    /// Language of the labels on the map.
    /// </summary>
    public string language = "en";

    /// <summary>
    /// Prohibits drawing of maps.\n
    /// <strong>
    /// Important: Do not forget to disable this restriction. \n
    /// Otherwise, the map will never be redrawn.
    /// </strong>
    /// </summary>
    public bool lockRedraw = false;

    /// <summary>
    /// A flag that indicates that need to redraw the map.
    /// </summary>
    public bool needRedraw;

    /// <summary>
    /// Not interact under the GUI.
    /// </summary>
    public bool notInteractUnderGUI = true;

    /// <summary>
    /// ID of current map type.
    /// </summary>
    public string mapType;

    /// <summary>
    /// URL of the proxy server used for Webplayer platform.
    /// </summary>
    public string proxyURL = "http://service.infinity-code.com/redirect.php?";

    /// <summary>
    /// A flag that indicates whether to redraw the map at startup.
    /// </summary>
    public bool redrawOnPlay;

    /// <summary>
    /// Render map in a separate thread. Recommended.
    /// </summary>
    public bool renderInThread = true;

    /// <summary>
    /// Template path in Resources, from where the tiles will be loaded.\n
    /// This field supports tokens.
    /// </summary>
    public string resourcesPath = "OnlineMapsTiles/{zoom}/{x}/{y}";

    /// <summary>
    /// Indicates when the marker will show tips.
    /// </summary>
    public OnlineMapsShowMarkerTooltip showMarkerTooltip = OnlineMapsShowMarkerTooltip.onHover;

    /// <summary>
    /// Specifies from where the tiles should be loaded (Online, Resources, Online and Resources).
    /// </summary>
    public OnlineMapsSource source = OnlineMapsSource.Online;

    /// <summary>
    /// Indicates that Unity need to stop playing when compiling scripts.
    /// </summary>
    public bool stopPlayingWhenScriptsCompile = true;

    /// <summary>
    /// Texture, which is used to draw the map. <br/>
    /// <strong>To change this value, use OnlineMaps.SetTexture.</strong>
    /// </summary>
    public Texture2D texture;

    [NonSerialized]
    public OnlineMapsTooltipDrawerBase tooltipDrawer;

    /// <summary>
    /// Background texture of tooltip.
    /// </summary>
    public Texture2D tooltipBackgroundTexture;

    /// <summary>
    /// Specifies whether to draw traffic.
    /// </summary>
    public bool traffic = false;

    /// <summary>
    /// Provider of traffic jams
    /// </summary>
    [NonSerialized]
    public OnlineMapsTrafficProvider trafficProvider;

    /// <summary>
    /// ID of current traffic provider.
    /// </summary>
    public string trafficProviderID = "googlemaps";

    /// <summary>
    /// Use only the current zoom level of the tiles.
    /// </summary>
    public bool useCurrentZoomTiles = false;

    /// <summary>
    /// Use a proxy server for WebGL?
    /// </summary>
    public bool useProxy = true;

    /// <summary>
    /// Specifies is necessary to use software JPEG decoder.
    /// Use only if you have problems with hardware decoding of JPEG.
    /// </summary>
    public bool useSoftwareJPEGDecoder = false;

    /// <summary>
    /// Map width in pixels.
    /// </summary>
    public int width = 1024;

    #endregion

    #region Private Fields

    [NonSerialized]
    private OnlineMapsProvider.MapType _activeType;

    [SerializeField]
    private string _activeTypeSettings;

    [NonSerialized]
    private OnlineMapsBuffer _buffer;

    private OnlineMapsControlBase _control;

    private bool _labels;
    private string _language;
    private string _mapType;

    private OnlineMapsPositionRange _positionRange;
    private OnlineMapsProjection _projection;
    private bool _traffic;
    private string _trafficProviderID;

    [SerializeField]
    private float _zoom = MINZOOM;

    private OnlineMapsRange _zoomRange;

    private double bottomRightLatitude;
    private double bottomRightLongitude;
    private Color[] defaultColors;
    private int izoom = MINZOOM;

    [SerializeField]
    private double latitude = 0;

    [SerializeField]
    private double longitude = 0;

#if NETFX_CORE
    private OnlineMapsThreadWINRT renderThread;
#elif !UNITY_WEBGL
    private Thread renderThread;
#endif

    private OnlineMapsSavableItem[] savableItems;

    private double topLeftLatitude;
    private double topLeftLongitude;

    #endregion
    #endregion

    #region Properties

    #region Static  Properties

    /// <summary>
    /// Singleton instance of map.
    /// </summary>
    public static OnlineMaps instance
    {
        get { return _instance; }
    }

    #endregion

    #region Public  Properties

    /// <summary>
    /// Active type of map.
    /// </summary>
    public OnlineMapsProvider.MapType activeType
    {
        get
        {
            if (_activeType == null || _activeType.fullID != mapType)
            {
                _activeType = OnlineMapsProvider.FindMapType(mapType);
                _projection = _activeType.provider.projection;
                mapType = _activeType.fullID;
            }
            return _activeType;
        }
        set
        {
            if (_activeType == value) return;

            _activeType = value;
            _projection = _activeType.provider.projection;
            _mapType = mapType = value.fullID;

            if (isPlaying) RedrawImmediately();
        }
    }

    /// <summary>
    /// Gets the bottom right position.
    /// </summary>
    /// <value>
    /// The bottom right position.
    /// </value>
    public Vector2 bottomRightPosition
    {
        get
        {
            if (Math.Abs(bottomRightLatitude) < double.Epsilon && Math.Abs(bottomRightLongitude) < double.Epsilon) UpdateCorners();
            return new Vector2((float)bottomRightLongitude, (float)bottomRightLatitude);
        }
    }

    /// <summary>
    /// Gets the coordinates of the map view.
    /// </summary>
    public OnlineMapsGeoRect bounds
    {
        get
        {
            return new OnlineMapsGeoRect(topLeftLongitude, topLeftLatitude, bottomRightLongitude, bottomRightLatitude);
        }
    }

    /// <summary>
    /// Reference to the current draw buffer.
    /// </summary>
    public OnlineMapsBuffer buffer
    {
        get
        {
            if (_buffer == null) _buffer = new OnlineMapsBuffer(this);
            return _buffer;
        }
    }

    /// <summary>
    /// The current state of the drawing buffer.
    /// </summary>
    public OnlineMapsBufferStatus bufferStatus
    {
        get { return buffer.status; }
    }

    /// <summary>
    /// Display control script.
    /// </summary>
    public OnlineMapsControlBase control
    {
        get
        {
            if (_control == null) _control = GetComponent<OnlineMapsControlBase>();
            return _control;
        }
    }

    /// <summary>
    /// Gets and sets float zoom value
    /// </summary>
    public float floatZoom
    {
        get { return _zoom; }
        set
        {
            if (Mathf.Abs(_zoom - value) < float.Epsilon) return;

            float z = Mathf.Clamp(value, MINZOOM, MAXZOOM);
            if (zoomRange != null) z = zoomRange.CheckAndFix(z);
            z = CheckMapSize(z);
            if (Math.Abs(_zoom - z) < float.Epsilon) return;

            _zoom = z;
            izoom = (int) z;
            SetPosition(longitude, latitude);
            UpdateCorners();
            allowRedraw = true;
            needRedraw = true;
            DispatchEvent(OnlineMapsEvents.changedZoom);
        }
    }

    /// <summary>
    /// Coordinates of the center point of the map.
    /// </summary>
    public Vector2 position
    {
        get { return new Vector2((float)longitude, (float)latitude); }
        set
        {
            SetPosition(value.x, value.y);
        }
    }

    /// <summary>
    /// Limits the range of map coordinates.
    /// </summary>
    public OnlineMapsPositionRange positionRange
    {
        get { return _positionRange; }
        set
        {
            _positionRange = value;
            if (value != null) value.CheckAndFix(ref longitude, ref latitude);
        }
    }

    /// <summary>
    /// Projection of active provider.
    /// </summary>
    public OnlineMapsProjection projection
    {
        get
        {
            if (_projection == null) _projection = activeType.provider.projection;
            return _projection;
        }
    }

    /// <summary>
    /// Gets the top left position.
    /// </summary>
    /// <value>
    /// The top left position.
    /// </value>
    public Vector2 topLeftPosition
    {
        get
        {
            if (Math.Abs(topLeftLatitude) < double.Epsilon && Math.Abs(topLeftLongitude) < double.Epsilon) UpdateCorners();

            return new Vector2((float)topLeftLongitude, (float)topLeftLatitude);
        }
    }

    /// <summary>
    /// Current zoom.
    /// </summary>
    public int zoom
    {
        get
        {
            if (izoom == 0) izoom = (int) _zoom;
            return izoom;
        }
        set { floatZoom = value; }
    }

    /// <summary>
    /// The scaling factor for zoom
    /// </summary>
    public float zoomCoof
    {
        get { return 1 - zoomScale / 2; }
    }

    /// <summary>
    /// Specifies the valid range of map zoom.
    /// </summary>
    public OnlineMapsRange zoomRange
    {
        get { return _zoomRange; }
        set
        {
            _zoomRange = value;
            if (value != null) floatZoom = value.CheckAndFix(floatZoom);
        }
    }

    /// <summary>
    /// The fractional part of zoom
    /// </summary>
    public float zoomScale
    {
        get { return _zoom - zoom; }
    }

    #endregion
    #endregion

    #region Methods

    public void Awake()
    {
        _instance = this;

        if (control == null)
        {
            Debug.LogError("Can not find a Control.");
            return;
        }

        if (control.resultIsTexture)
        {
            if (texture != null)
            {
                width = texture.width;
                height = texture.height;
            }
        }
        else
        {
            texture = null;
        }

        izoom = (int)floatZoom;
        UpdateCorners();

        control.OnAwakeBefore();

        if (control.resultIsTexture)
        {
            if (texture != null) defaultColors = texture.GetPixels();

            if (defaultTileTexture == null)
            {
                OnlineMapsRasterTile.defaultColors = new Color32[OnlineMapsUtils.sqrTileSize];
                for (int i = 0; i < OnlineMapsUtils.sqrTileSize; i++) OnlineMapsRasterTile.defaultColors[i] = emptyColor;
            }
            else OnlineMapsRasterTile.defaultColors = defaultTileTexture.GetPixels32();
        }

        SetPosition(longitude, latitude);
    }

    private void CheckBaseProps()
    {
        if (mapType != _mapType)
        {
            activeType = OnlineMapsProvider.FindMapType(mapType);
            _mapType = mapType = activeType.fullID;
            if (_buffer != null) _buffer.UnloadOldTypes();
            Redraw();
       }

        if (_language != language || _labels != labels)
        {
            _labels = labels;
            _language = language;

            if (_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
#if NETFX_CORE
                if (renderThread != null) renderThread.Dispose();
#endif
#if !UNITY_WEBGL
                renderThread = null;
#endif
            }
            
            Redraw();
        }
        if (traffic != _traffic || trafficProviderID != _trafficProviderID)
        {
            _traffic = traffic;

            _trafficProviderID = trafficProviderID;
            trafficProvider = OnlineMapsTrafficProvider.GetByID(trafficProviderID);

            OnlineMapsTile[] tiles;
            lock (OnlineMapsTile.lockTiles)
            {
                tiles = OnlineMapsTile.tiles.ToArray();
            }
            if (traffic)
            {
                foreach (OnlineMapsTile tile in tiles)
                {
                    OnlineMapsRasterTile rTile = tile as OnlineMapsRasterTile;
                    rTile.trafficProvider = trafficProvider;
                    rTile.trafficWWW = new OnlineMapsWWW(rTile.trafficURL);
                    rTile.trafficWWW["tile"] = tile;
                    rTile.trafficWWW.OnComplete += OnlineMapsTileManager.OnTrafficWWWComplete;
                    if (rTile.trafficTexture != null)
                    {
                        OnlineMapsUtils.Destroy(rTile.trafficTexture);
                        rTile.trafficTexture = null;
                    }
                }
            }
            else
            {
                foreach (OnlineMapsTile tile in tiles)
                {
                    OnlineMapsRasterTile rTile = tile as OnlineMapsRasterTile;
                    if (rTile.trafficTexture != null)
                    {
                        OnlineMapsUtils.Destroy(rTile.trafficTexture);
                        rTile.trafficTexture = null;
                    }
                    rTile.trafficWWW = null;
                }
            }
            Redraw();
        }
    }

    private void CheckBufferComplete()
    {
        if (buffer.status != OnlineMapsBufferStatus.complete) return;
        if (buffer.needUnloadTiles)
            buffer.UnloadOldTiles();

        OnlineMapsTile.UnloadUnusedTiles();

        if (allowRedraw)
        {
            if (control.resultIsTexture)
            {
                if (texture != null)
                {
                    texture.SetPixels32(buffer.frontBuffer);
                    texture.Apply(false);
                    if (control.activeTexture != texture) control.SetTexture(texture);
                }
            }

            if (OnlineMapsTileManager.OnPreloadTiles != null) OnlineMapsTileManager.OnPreloadTiles();
            if (control is OnlineMapsControlBase3D) (control as OnlineMapsControlBase3D).UpdateControl();

            if (OnMapUpdated != null) OnMapUpdated();
        }

        buffer.status = OnlineMapsBufferStatus.wait;
    }

    public float CheckMapSize(float z)
    {
        int iz = Mathf.FloorToInt(z);
        int max = (1 << iz) * OnlineMapsUtils.tileSize;
        if (max < width || max < height) return CheckMapSize(iz + 1);

        return z;
    }

#if UNITY_EDITOR
    private void CheckScriptCompiling()
    {
        isPlaying = EditorApplication.isPlaying;
        if (!isPlaying) EditorApplication.update -= CheckScriptCompiling;

        if (stopPlayingWhenScriptsCompile && isPlaying && EditorApplication.isCompiling)
        {
            Debug.Log("Online Maps stop playing to compile scripts.");
            EditorApplication.isPlaying = false;
        }
    }
#endif

    /// <summary>
    /// Allows you to test the connection to the Internet.
    /// </summary>
    /// <param name="callback">Function, which will return the availability of the Internet.</param>
    public void CheckServerConnection(Action<bool> callback)
    {
        OnlineMapsTile tempTile = control.CreateTile(350, 819, 11, false);
        string url = tempTile.url;
        tempTile.Dispose();

        OnlineMapsWWW checkConnectionWWW = new OnlineMapsWWW(url);
        checkConnectionWWW.OnComplete += www =>
        {
            callback(!www.hasError);
        };
    }

    /// <summary>
    /// Dispatch map events.
    /// </summary>
    /// <param name="evs">Events you want to dispatch.</param>
    public void DispatchEvent(params OnlineMapsEvents[] evs)
    {
        if (!dispatchEvents) return;

        foreach (OnlineMapsEvents ev in evs)
        {
            if (ev == OnlineMapsEvents.changedPosition && OnChangePosition != null) OnChangePosition();
            else if (ev == OnlineMapsEvents.changedZoom && OnChangeZoom != null) OnChangeZoom();
        }
    }

    private void FixPositionUsingBorders(ref double lng, ref double lat, int countX, int countY)
    {
        double px, py;
        projection.CoordinatesToTile(lng, lat, zoom, out px, out py);
        double ox = countX / 2d;
        double oy = countY / 2d;

        double tlx, tly, brx, bry;

        projection.TileToCoordinates(px - ox, py - oy, zoom, out tlx, out tly);
        projection.TileToCoordinates(px + ox, py + oy, zoom, out brx, out bry);

        bool tlxc = false;
        bool tlyc = false;
        bool brxc = false;
        bool bryc = false;

        if (tlx < positionRange.minLng)
        {
            tlxc = true;
            tlx = positionRange.minLng;
        }
        if (brx > positionRange.maxLng)
        {
            brxc = true;
            brx = positionRange.maxLng;
        }
        if (tly > positionRange.maxLat)
        {
            tlyc = true;
            tly = positionRange.maxLat;
        }
        if (bry < positionRange.minLat)
        {
            bryc = true;
            bry = positionRange.minLat;
        }

        double tmp;
        bool recheckX = false, recheckY = false;

        if (tlxc && brxc)
        {
            double tx1, tx2;
            projection.CoordinatesToTile(positionRange.minLng, positionRange.maxLat, zoom, out tx1, out tmp);
            projection.CoordinatesToTile(positionRange.maxLng, positionRange.minLat, zoom, out tx2, out tmp);
            px = (tx1 + tx2) / 2;
        }
        else if (tlxc)
        {
            projection.CoordinatesToTile(tlx, tly, zoom, out px, out tmp);
            px += ox;
            recheckX = true;
        }
        else if (brxc)
        {
            projection.CoordinatesToTile(brx, bry, zoom, out px, out tmp);
            px -= ox;
            recheckX = true;
        }

        if (tlyc && bryc)
        {
            double ty1, ty2;
            projection.CoordinatesToTile(positionRange.minLng, positionRange.maxLat, zoom, out tmp, out ty1);
            projection.CoordinatesToTile(positionRange.maxLng, positionRange.minLat, zoom, out tmp, out ty2);
            py = (ty1 + ty2) / 2;
        }
        else if (tlyc)
        {
            projection.CoordinatesToTile(tlx, tly, zoom, out tmp, out py);
            py += oy;
            recheckY = true;
        }
        else if (bryc)
        {
            projection.CoordinatesToTile(brx, bry, zoom, out tmp, out py);
            py -= oy;
            recheckY = true;
        }

        if (recheckX || recheckY)
        {
            projection.TileToCoordinates(px - ox, py - oy, zoom, out tlx, out tly);
            projection.TileToCoordinates(px + ox, py + oy, zoom, out brx, out bry);
            bool centerX = false, centerY = false;
            if (tlx < positionRange.minLng && brxc) centerX = true;
            else if (brx > positionRange.maxLng && tlxc) centerX = true;

            if (tly > positionRange.maxLat && bryc) centerY = true;
            else if (bry < positionRange.minLat && tlyc) centerY = true;

            if (centerX)
            {
                double tx1, tx2;
                projection.CoordinatesToTile(positionRange.minLng, positionRange.maxLat, zoom, out tx1, out tmp);
                projection.CoordinatesToTile(positionRange.maxLng, positionRange.minLat, zoom, out tx2, out tmp);
                px = (tx1 + tx2) / 2;
            }
            if (centerY)
            {
                double ty1, ty2;
                projection.CoordinatesToTile(positionRange.minLng, positionRange.maxLat, zoom, out tmp, out ty1);
                projection.CoordinatesToTile(positionRange.maxLng, positionRange.minLat, zoom, out tmp, out ty2);
                py = (ty1 + ty2) / 2;
            }
        }

        if (tlxc || brxc || tlyc || bryc) projection.TileToCoordinates(px, py, zoom, out lng, out lat);
    }

    /// <summary>
    /// Get the bottom-right corner of the map.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    public void GetBottomRightPosition(out double lng, out double lat)
    {
        if (Math.Abs(bottomRightLatitude) < double.Epsilon && Math.Abs(bottomRightLongitude) < double.Epsilon) UpdateCorners();
        lng = bottomRightLongitude;
        lat = bottomRightLatitude;
    }

    /// <summary>
    /// Returns the coordinates of the corners of the map
    /// </summary>
    /// <param name="tlx">Longitude of the left border</param>
    /// <param name="tly">Latitude of the top border</param>
    /// <param name="brx">Longitude of the right border</param>
    /// <param name="bry">Latitude of the bottom border</param>
    public void GetCorners(out double tlx, out double tly, out double brx, out double bry)
    {
        if (Math.Abs(bottomRightLatitude) < double.Epsilon && Math.Abs(bottomRightLongitude) < double.Epsilon || Math.Abs(topLeftLatitude) < double.Epsilon && Math.Abs(topLeftLongitude) < double.Epsilon) UpdateCorners();

        brx = bottomRightLongitude;
        bry = bottomRightLatitude;
        tlx = topLeftLongitude;
        tly = topLeftLatitude;
    }

    /// <summary>
    /// Gets drawing element from screen.
    /// </summary>
    /// <param name="screenPosition">Screen position.</param>
    /// <returns>Drawing element</returns>
    public OnlineMapsDrawingElement GetDrawingElement(Vector2 screenPosition)
    {
        Vector2 coords = OnlineMapsControlBase.instance.GetCoords(screenPosition);
        return OnlineMapsDrawingElementManager.instance.LastOrDefault(el => el.HitTest(coords, zoom));
    }

    /// <summary>
    /// Get the map coordinate.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    public void GetPosition(out double lng, out double lat)
    {
        lat = latitude;
        lng = longitude;
    }

    public OnlineMapsSavableItem[] GetSavableItems()
    {
        if (savableItems != null) return savableItems;

        savableItems = new[]
        {
            new OnlineMapsSavableItem("map", "Map settings", SaveSettings)
            {
                priority = 100,
                loadCallback = Load
            }
        };

        return savableItems;
    }

    /// <summary>
    /// Get the tile coordinates of the corners of the map
    /// </summary>
    /// <param name="tlx">Left tile X</param>
    /// <param name="tly">Top tile Y</param>
    /// <param name="brx">Right tile X</param>
    /// <param name="bry">Bottom tile Y</param>
    public void GetTileCorners(out double tlx, out double tly, out double brx, out double bry)
    {
        GetTileCorners(out tlx, out tly, out brx, out bry, zoom);
    }

    /// <summary>
    /// Get the tile coordinates of the corners of the map
    /// </summary>
    /// <param name="tlx">Left tile X</param>
    /// <param name="tly">Top tile Y</param>
    /// <param name="brx">Right tile X</param>
    /// <param name="bry">Bottom tile Y</param>
    /// <param name="zoom">Zoom</param>
    public void GetTileCorners(out double tlx, out double tly, out double brx, out double bry, int zoom)
    {
        if (Math.Abs(bottomRightLatitude) < double.Epsilon && Math.Abs(bottomRightLongitude) < double.Epsilon || 
            Math.Abs(topLeftLatitude) < double.Epsilon && Math.Abs(topLeftLongitude) < double.Epsilon) UpdateCorners();

        projection.CoordinatesToTile(topLeftLongitude, topLeftLatitude, zoom, out tlx, out tly);
        projection.CoordinatesToTile(bottomRightLongitude, bottomRightLatitude, zoom, out brx, out bry);
    }

    /// <summary>
    /// Get the tile coordinates of the map
    /// </summary>
    /// <param name="px">Tile X</param>
    /// <param name="py">Tile Y</param>
    public void GetTilePosition(out double px, out double py)
    {
        projection.CoordinatesToTile(longitude, latitude, zoom, out px, out py);
    }

    /// <summary>
    /// Get the tile coordinates of the map
    /// </summary>
    /// <param name="px">Tile X</param>
    /// <param name="py">Tile Y</param>
    /// <param name="zoom">Zoom</param>
    public void GetTilePosition(out double px, out double py, int zoom)
    {
        projection.CoordinatesToTile(longitude, latitude, zoom, out px, out py);
    }

    /// <summary>
    /// Get the top-left corner of the map.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    public void GetTopLeftPosition(out double lng, out double lat)
    {
        if (Math.Abs(topLeftLatitude) < double.Epsilon && Math.Abs(topLeftLongitude) < double.Epsilon) UpdateCorners();
        lng = topLeftLongitude;
        lat = topLeftLatitude;
    }

    private void LateUpdate()
    {
        if (OnLateUpdateBefore != null) OnLateUpdateBefore();

        if (control == null || lockRedraw) return;
        StartBuffer();
        CheckBufferComplete();

        if (OnLateUpdateAfter != null) OnLateUpdateAfter();
    }

    public void Load(OnlineMapsJSONItem json)
    {
        (json as OnlineMapsJSONObject).DeserializeObject(this, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        izoom = (int) _zoom;
    }

    public void OnAfterDeserialize()
    {
        try
        {
            activeType.LoadSettings(_activeTypeSettings);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(exception.Message + "\n" + exception.StackTrace);
        }
    }

    public void OnBeforeSerialize()
    {
        _activeTypeSettings = activeType.GetSettings();
    }

    private void OnDestroy()
    {
        OnlineMapsThreadManager.Dispose();

        if (_buffer != null)
        {
            _buffer.Dispose();
            _buffer = null;
        }
#if NETFX_CORE
        if (renderThread != null) renderThread.Dispose();
#endif
#if !UNITY_WEBGL
        renderThread = null;
#endif
        _control = null;

        if (defaultColors != null && texture != null)
        {
            if (texture.width * texture.height == defaultColors.Length)
            {
                texture.SetPixels(defaultColors);
                texture.Apply();
            }
        }
    }

    private void OnDisable ()
    {
        OnlineMapsThreadManager.Dispose();

        if (_buffer != null)
        {
            _buffer.Dispose();
            _buffer = null;
        }

#if NETFX_CORE
        if (renderThread != null) renderThread.Dispose();
#endif
#if !UNITY_WEBGL
        renderThread = null;
#endif

        _control = null;
        OnChangePosition = null;
        OnChangeZoom = null;
        OnMapUpdated = null;
        OnUpdateBefore = null;
        OnUpdateLate = null;
        OnlineMapsTile.OnGetResourcesPath = null;
        OnlineMapsTile.OnTileDownloaded = null;
        OnlineMapsTile.OnTrafficDownloaded = null;
        OnlineMapsMarkerBase.OnMarkerDrawTooltip = null;

        if (_instance == this) _instance = null;
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.update += CheckScriptCompiling;
#endif

        OnlineMapsUtils.persistentDataPath = Application.persistentDataPath;

        isPlaying = true;
        _instance = this;

        tooltipDrawer = new OnlineMapsGUITooltipDrawer(this);

        activeType = OnlineMapsProvider.FindMapType(mapType);
        _mapType = mapType = activeType.fullID;

        trafficProvider = OnlineMapsTrafficProvider.GetByID(trafficProviderID);

        if (language == "") language = activeType.provider.twoLetterLanguage ? "en" : "eng";

        _language = language;
        _labels = labels;
        _traffic = traffic;
        _trafficProviderID = trafficProviderID;
        izoom = (int) _zoom;

        UpdateCorners();
    }

#if !ONLINEMAPS_NOGUI
    private void OnGUI()
    {
        if (OnGUIBefore != null) OnGUIBefore();
        if (OnGUIAfter != null) OnGUIAfter();
    }
#endif

    /// <summary>
    /// Full redraw map.
    /// </summary>
    public void Redraw()
    {
        needRedraw = true;
        allowRedraw = true;
    }

    /// <summary>
    /// Stops the current process map generation, clears all buffers and completely redraws the map.
    /// </summary>
    public void RedrawImmediately()
    {
        OnlineMapsThreadManager.Dispose();

        if (renderInThread)
        {
            if (_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
            }

#if NETFX_CORE
            if (renderThread != null) renderThread.Dispose();
#endif
#if !UNITY_WEBGL
            renderThread = null;
#endif
        }
        else StartBuffer();

        Redraw();
    }

    private OnlineMapsJSONItem SaveSettings()
    {
        OnlineMapsJSONObject json = OnlineMapsJSON.Serialize(new {
            longitude,
            latitude,
            floatZoom,
            source,
            mapType,
            labels,
            traffic,
            redrawOnPlay,
            emptyColor,
            defaultTileTexture,
            tooltipBackgroundTexture,
            showMarkerTooltip,
            useSoftwareJPEGDecoder,
            useCurrentZoomTiles
        }) as OnlineMapsJSONObject;

        if (activeType.isCustom) json.Add("customProviderURL", customProviderURL);

        if (control.resultIsTexture)
        {
            defaultColors = texture.GetPixels();
            json.Add("texture", texture);
        }
        else
        {
            json.AppendObject(new
            {
                width,
                height
            });
        }

        return json;
    }

    /// <summary>
    /// Set the the map coordinate.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    public void SetPosition(double lng, double lat)
    {
        if (width == 0 && height == 0)
        {
            if (control.resultIsTexture && texture != null)
            {
                width = texture.width;
                height = texture.height;
            }
        }
        int countX = width / OnlineMapsUtils.tileSize;
        int countY = height / OnlineMapsUtils.tileSize;

        if (lng < -180) lng += 360;
        else if (lng > 180) lng -= 360;

        if (positionRange != null)
        {
            if (positionRange.type == OnlineMapsPositionRangeType.center) positionRange.CheckAndFix(ref lng, ref lat);
            else if (positionRange.type == OnlineMapsPositionRangeType.border) FixPositionUsingBorders(ref lng, ref lat, countX, countY);
        }

        double tpx, tpy;
        projection.CoordinatesToTile(lng, lat, zoom, out tpx, out tpy);

        float haftCountY = countY / 2f * zoomCoof;
        int maxY = 1 << zoom;
        bool modified = false;
        if (tpy < haftCountY)
        {
            tpy = haftCountY;
            modified = true;
        }
        else if (tpy + haftCountY >= maxY)
        {
            tpy = maxY - haftCountY;
            modified = true;
        }

        if (modified) projection.TileToCoordinates(tpx, tpy, zoom, out lng, out lat);

        if (Math.Abs(latitude - lat) < double.Epsilon && Math.Abs(longitude - lng) < double.Epsilon) return;

        allowRedraw = true;
        needRedraw = true;

        latitude = lat;
        longitude = lng;
        UpdateCorners();

        DispatchEvent(OnlineMapsEvents.changedPosition);
    }

    /// <summary>
    /// Sets the position and zoom.
    /// </summary>
    /// <param name="lng">Longitude</param>
    /// <param name="lat">Latitude</param>
    /// <param name="ZOOM">Zoom</param>
    public void SetPositionAndZoom(double lng, double lat, float? ZOOM = null)
    {
        if (ZOOM.HasValue) floatZoom = ZOOM.Value;
        SetPosition(lng, lat);
    }

    /// <summary>
    /// Sets the texture, which will draw the map.
    /// Texture displaying on the source you need to change yourself.
    /// </summary>
    /// <param name="newTexture">Texture, where you want to draw the map.</param>
    public void SetTexture(Texture2D newTexture)
    {
        texture = newTexture;
        width = texture.width;
        height = texture.height;

        float z = CheckMapSize(floatZoom);
        if (Math.Abs(floatZoom - z) > float.Epsilon) floatZoom = z;

        control.SetTexture(texture);

        allowRedraw = true;
        needRedraw = true;
    }

    /// <summary>
    /// Sets the position of the center point of the map based on the tile position.
    /// </summary>
    /// <param name="tx">Tile X</param>
    /// <param name="ty">Tile Y</param>
    /// <param name="tileZoom">Tile zoom</param>
    public void SetTilePosition(double tx, double ty, int? tileZoom = null)
    {
        double lng, lat;
        projection.TileToCoordinates(tx, ty, tileZoom != null ? tileZoom.Value : zoom, out lng, out lat);
        SetPosition(lng, lat);
    }

    private void Start()
    {
        if (redrawOnPlay) allowRedraw = true;
        needRedraw = true;
        _zoom = CheckMapSize(_zoom);
    }

    private void StartBuffer()
    {
        if (!allowRedraw || !needRedraw) return;
        if (buffer.status != OnlineMapsBufferStatus.wait) return;

        if (latitude < -90) latitude = -90;
        else if (latitude > 90) latitude = 90;
        while (longitude < -180 || longitude > 180)
        {
            if (longitude < -180) longitude += 360;
            else if (longitude > 180) longitude -= 360;
        }
        
        buffer.status = OnlineMapsBufferStatus.start;

        if (!control.resultIsTexture) renderInThread = false;

#if !UNITY_WEBGL
        if (renderInThread)
        {
            if (renderThread == null)
            {
#if NETFX_CORE
                renderThread = new OnlineMapsThreadWINRT(buffer.GenerateFrontBuffer);
#else
                renderThread = new Thread(buffer.GenerateFrontBuffer);
#endif
                renderThread.Start();
            }
        }
        else buffer.GenerateFrontBuffer();
#else
        buffer.GenerateFrontBuffer();
#endif

        needRedraw = false;
    }
   
    private void Update()
    {
      
        OnlineMapsThreadManager.ExecuteMainThreadActions();

        if (OnUpdateBefore != null) OnUpdateBefore();
        
        CheckBaseProps();
        OnlineMapsTileManager.StartDownloading();

        if (OnUpdateLate != null) OnUpdateLate();
    }

    /// <summary>
    /// Updates the coordinates of the corners of the map
    /// </summary>
    public void UpdateCorners()
    {
        UpdateTopLeftPosition();
        UpdateBottonRightPosition();

        int max = (1 << izoom) * OnlineMapsUtils.tileSize;
        if (max == width && Mathf.Abs(zoomScale) < float.Epsilon)
        {
            double lng = longitude + 180;
            topLeftLongitude = lng + 0.001;
            if (topLeftLongitude > 180) topLeftLongitude -= 360;

            bottomRightLongitude = lng - 0.001;
            if (bottomRightLongitude > 180) bottomRightLongitude -= 360;
        }
    }

    private void UpdateBottonRightPosition()
    {
        int countX = width / OnlineMapsUtils.tileSize;
        int countY = height / OnlineMapsUtils.tileSize;

        double px, py;
        projection.CoordinatesToTile(longitude, latitude, zoom, out px, out py);

        px += countX / 2d * zoomCoof;
        py += countY / 2d * zoomCoof;

        projection.TileToCoordinates(px, py, zoom, out bottomRightLongitude, out bottomRightLatitude);
    }

    private void UpdateTopLeftPosition()
    {
        int countX = width / OnlineMapsUtils.tileSize;
        int countY = height / OnlineMapsUtils.tileSize;

        double px, py;

        projection.CoordinatesToTile(longitude, latitude, zoom, out px, out py);

        px -= countX / 2d * zoomCoof;
        py -= countY / 2d * zoomCoof;

        projection.TileToCoordinates(px, py, zoom, out topLeftLongitude, out topLeftLatitude);
    }

#endregion

#region Obsolete

    [Obsolete("Use OnlineMapsGUITooltipDrawer.OnPrepareTooltipStyleDelegate", true)]
    public delegate void OnPrepareTooltipStyleDelegate(ref GUIStyle style);

    [Obsolete]
    public bool useSmartTexture = true;

    [Obsolete]
    public Func<double, double, Texture2D, string, OnlineMapsMarker> OnAddMarker;

    [Obsolete]
    public Predicate<OnlineMapsMarker> OnRemoveMarker;

    [Obsolete]
    public Predicate<int> OnRemoveMarkerAt;

    [Obsolete("Inherit IOnlineMapsSavableComponent interface and add your MonoBehaviour to the map GameObject")]
    public Action OnSaveSettings;

    [Obsolete("Use OnlineMapsGUITooltipDrawer.OnPrepareTooltipStyle")]
    public OnlineMapsGUITooltipDrawer.OnPrepareTooltipStyleDelegate OnPrepareTooltipStyle
    {
        get { return OnlineMapsGUITooltipDrawer.OnPrepareTooltipStyle; }
        set { OnlineMapsGUITooltipDrawer.OnPrepareTooltipStyle = value; }
    }

    [Obsolete("Use OnlineMapsTileManager.OnStartDownloadTile")]
    public Action<OnlineMapsTile> OnStartDownloadTile
    {
        get { return OnlineMapsTileManager.OnStartDownloadTile; }
        set { OnlineMapsTileManager.OnStartDownloadTile = value; }
    }

    [Obsolete("Use OnlineMapsMarkerManager.defaultAlign")]
    public OnlineMapsAlign defaultMarkerAlign
    {
        get { return OnlineMapsMarkerManager.instance.defaultAlign; }
        set { OnlineMapsMarkerManager.instance.defaultAlign = value; }
    }

    [Obsolete("Use OnlineMapsMarkerManager.defaultTexture")]
    public Texture2D defaultMarkerTexture
    {
        get { return OnlineMapsMarkerManager.instance.defaultTexture; }
        set { OnlineMapsMarkerManager.instance.defaultTexture = value; }
    }

    [Obsolete("Use OnlineMapsDrawingElementManager.items")]
    public List<OnlineMapsDrawingElement> drawingElements
    {
        get { return OnlineMapsDrawingElementManager.instance.items; }
        set { OnlineMapsDrawingElementManager.SetItems(value); }
    }

    [Obsolete("Use OnlineMapsMarkerManager")]
    public OnlineMapsMarker[] markers
    {
        get { return OnlineMapsMarkerManager.instance.ToArray(); }
        set { OnlineMapsMarkerManager.SetItems(value); }
    }

    [Obsolete("Use OnlineMapsControlBase.resultIsTexture")]
    public OnlineMapsTarget target
    {
        get { return control.resultType; }
    }

    [Obsolete("Use height")]
    public int tilesetHeight
    {
        get { return height; }
        set { height = value; }
    }

    [Obsolete("Use OnlineMapsTileSetControl.sizeInScene")]
    public Vector2 tilesetSize
    {
        get { return OnlineMapsTileSetControl.instance.sizeInScene; }
        set { OnlineMapsTileSetControl.instance.sizeInScene = value; }
    }

    [Obsolete("Use width")]
    public int tilesetWidth
    {
        get { return width; }
        set { width = value; }
    }

    [Obsolete("Use OnlineMapsTooltipDrawerBase.tooltip")]
    public string tooltip
    {
        get { return OnlineMapsTooltipDrawerBase.tooltip; }
        set { OnlineMapsTooltipDrawerBase.tooltip = value; }
    }

    [Obsolete("Use OnlineMapsTooltipDrawerBase.tooltipDrawingElement")]
    public OnlineMapsDrawingElement tooltipDrawingElement
    {
        get { return OnlineMapsTooltipDrawerBase.tooltipDrawingElement; }
        set { OnlineMapsTooltipDrawerBase.tooltipDrawingElement = value; }
    }

    [Obsolete("Use OnlineMapsTooltipDrawerBase.tooltipMarker")]
    public OnlineMapsMarkerBase tooltipMarker
    {
        get { return OnlineMapsTooltipDrawerBase.tooltipMarker; }
        set { OnlineMapsTooltipDrawerBase.tooltipMarker = value; }
    }

    [Obsolete("Use useProxy")]
    public bool useWebplayerProxy
    {
        get { return useProxy; }
        set { useProxy = value; }
    }

    [Obsolete("Use proxyURL")]
    public string webplayerProxyURL
    {
        get { return proxyURL; }
        set { proxyURL = value; }
    }

    [Obsolete("Use OnlineMapsDrawingElementManager.AddItem")]
    public void AddDrawingElement(OnlineMapsDrawingElement element)
    {
        if (element.Validate())
        {
            drawingElements.Add(element);
            needRedraw = allowRedraw = true;
        }
    }

    [Obsolete("Use OnlineMapsMarkerManager.AddItem")]
    public OnlineMapsMarker AddMarker(OnlineMapsMarker marker)
    {
        return OnlineMapsMarkerManager.AddItem(marker);
    }

    [Obsolete("Use OnlineMapsMarkerManager.CreateItem")]
    public OnlineMapsMarker AddMarker(Vector2 markerPosition, string label)
    {
        return AddMarker(markerPosition.x, markerPosition.y, null, label);
    }

    [Obsolete("Use OnlineMapsMarkerManager.CreateItem")]
    public OnlineMapsMarker AddMarker(double markerLng, double markerLat, string label)
    {
        return AddMarker(markerLng, markerLat, null, label);
    }

    [Obsolete("Use OnlineMapsMarkerManager.CreateItem")]
    public OnlineMapsMarker AddMarker(Vector2 markerPosition, Texture2D markerTexture = null, string label = "")
    {
        return AddMarker(markerPosition.x, markerPosition.y, markerTexture, label);
    }

    [Obsolete("Use OnlineMapsMarkerManager.CreateItem")]
    public OnlineMapsMarker AddMarker(double markerLng, double markerLat, Texture2D markerTexture = null, string label = "")
    {
        return OnlineMapsMarkerManager.CreateItem(markerLng, markerLat, markerTexture, label);
    }

    [Obsolete("Use OnlineMapsMarkerManager.AddItems")]
    public void AddMarkers(OnlineMapsMarker[] newMarkers)
    {
        OnlineMapsMarkerManager.AddItems(newMarkers);
        for (int i = 0; i < newMarkers.Length; i++) newMarkers[i].Init();

        needRedraw = allowRedraw = true;
    }

    [Obsolete("Use OnlineMapsControlBase.markerDrawer.GetMarkerFromScreen")]
    public OnlineMapsMarker GetMarkerFromScreen(Vector2 screenPosition)
    {
        return control.markerDrawer.GetMarkerFromScreen(screenPosition);
    }

    [Obsolete("Use Use OnlineMapsDrawingElementManager.RemoveAllItems")]
    public void RemoveAllDrawingElements()
    {
        foreach (OnlineMapsDrawingElement element in drawingElements)
        {
            element.OnRemoveFromMap();
            element.Dispose();
        }
        drawingElements.Clear();
        needRedraw = true;
    }

    [Obsolete("Use OnlineMapsMarkerManager.RemoveAllItems")]
    public void RemoveAllMarkers()
    {
        OnlineMapsMarkerManager.RemoveAllItems();
        Redraw();
    }

    [Obsolete("Use Use OnlineMapsDrawingElementManager.RemoveItem")]
    public void RemoveDrawingElement(OnlineMapsDrawingElement element, bool disposeElement = true)
    {
        element.OnRemoveFromMap();
        if (disposeElement) element.Dispose();
        drawingElements.Remove(element);
        needRedraw = true;
    }

    [Obsolete("Use Use OnlineMapsDrawingElementManager.RemoveItemAt")]
    public void RemoveDrawingElementAt(int elementIndex)
    {
        if (elementIndex < 0 || elementIndex >= drawingElements.Count) return;

        OnlineMapsDrawingElement element = drawingElements[elementIndex];
        element.Dispose();

        element.OnRemoveFromMap();
        drawingElements.Remove(element);
        needRedraw = true;
    }

    [Obsolete("Use OnlineMapsMarkerManager.RemoveItem")]
    public void RemoveMarker(OnlineMapsMarker marker, bool disposeMarker = true)
    {
        if (OnRemoveMarker != null && OnRemoveMarker(marker)) return;

        OnlineMapsMarkerManager.RemoveItem(marker);
        if (disposeMarker) marker.Dispose();

        Redraw();
    }

    [Obsolete("Use OnlineMapsMarkerManager.RemoveItemAt")]
    public void RemoveMarkerAt(int markerIndex)
    {
        if (OnRemoveMarkerAt != null && OnRemoveMarkerAt(markerIndex)) return;

        OnlineMapsMarker marker = OnlineMapsMarkerManager.RemoveItemAt(markerIndex);
        if (marker != null) marker.Dispose();
        Redraw();
    }

    [Obsolete("Use OnlineMapsMarkerManager.RemoveItemsByTag")]
    public void RemoveMarkersByTag(params string[] tags)
    {
        OnlineMapsMarkerManager.RemoveItemsByTag(tags);
        Redraw();
    }

    [Obsolete("Use OnlineMapsTileManager.StartDownloadTile")]
    public void StartDownloadTile(OnlineMapsTile tile)
    {
        OnlineMapsTileManager.StartDownloadTile(tile);
    }

#endregion
}