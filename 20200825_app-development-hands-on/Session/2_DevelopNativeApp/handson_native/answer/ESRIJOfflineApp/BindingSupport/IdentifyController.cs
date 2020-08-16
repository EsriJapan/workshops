using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ESRIJOfflineApp.BindingSupport
{
    public class IdentifyController : DependencyObject
    {
        private WeakReference<MapView> _mapViewWeakRef;
        private bool _isIdentifyInProgress = false;
        private bool _wasMapViewDoubleTapped;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifyController"/> class.
        /// </summary>
        public IdentifyController()
        {
        }

        /// <summary>
        /// Gets or sets the MapView on which to perform identify operations
        /// </summary>
        public MapView MapView
        {
            get
            {
                MapView mapView = null;
                _mapViewWeakRef?.TryGetTarget(out mapView);
                return mapView;
            }
            set
            {
                var mapView = MapView;
                if (mapView != value)
                {
                    var oldMapView = mapView;
                    var newMapView = value;
                    if (oldMapView != null)
                    {
                        oldMapView.GeoViewTapped -= MapView_Tapped;
                        oldMapView.GeoViewDoubleTapped -= MapView_DoubleTapped;
                    }

                    if (_mapViewWeakRef == null)
                    {
                        _mapViewWeakRef = new WeakReference<MapView>(newMapView);
                    }
                    else
                    {
                        _mapViewWeakRef.SetTarget(newMapView);
                    }

                    newMapView.GeoViewTapped += MapView_Tapped;
                    newMapView.GeoViewDoubleTapped += MapView_DoubleTapped;
                }
            }
        }

        /// <summary>
        /// Invoked when GeoViewDoubleTapped event is firing
        /// </summary>
        private void MapView_DoubleTapped(object sender, GeoViewInputEventArgs e)
        {
            // set flag to true to help distinguish between a double tap and a single tap
            _wasMapViewDoubleTapped = true;
        }

        /// <summary>
        /// Invoked when GeoViewTapped event is firing
        /// </summary>
        private async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            _isIdentifyInProgress = true;
            var mapView = (MapView)sender;
            var target = Target;

            // Wait for double tap to fire
            // Identify is only peformed on single tap. The delay is used to detect and ignore double taps
            await Task.Delay(500);

            // If view has been double tapped, set tapped to handled and flag back to false
            // If view has been tapped just once, perform identify
            if (_wasMapViewDoubleTapped == true)
            {
                e.Handled = true;
                _wasMapViewDoubleTapped = false;
            }
            else
            {
                if (target is ILoadable loadable && loadable.LoadStatus == LoadStatus.NotLoaded)
                {
                    await loadable.LoadAsync();
                }

                // get the tap location in screen units
                var tapScreenPoint = e.Position;

                // set identify parameters
                var pixelTolerance = 10;
                var returnPopupsOnly = true;
                var maxResultCount = 10;

                IReadOnlyList<IdentifyLayerResult> layerResults = null;
                IReadOnlyList<IdentifyGraphicsOverlayResult> graphicsOverlayResults = null;
                try
                {
                    if (target == null)
                    {
                        // An identify target is not specified, so identify all layers and overlays
                        var identifyLayersTask = mapView.IdentifyLayersAsync(tapScreenPoint, pixelTolerance, returnPopupsOnly, maxResultCount);
                        var identifyOverlaysTask = mapView.IdentifyGraphicsOverlaysAsync(tapScreenPoint, pixelTolerance, returnPopupsOnly, maxResultCount);
                        await Task.WhenAll(identifyLayersTask, identifyOverlaysTask);
                        layerResults = identifyLayersTask.Result;
                        graphicsOverlayResults = identifyOverlaysTask.Result;
                    }
                    else if (target is Layer targetLayer)
                    {
                        // identify features in the target layer, passing the tap point, tolerance, types to return, and max results
                        var identifyResult = await mapView.IdentifyLayerAsync(targetLayer, tapScreenPoint, pixelTolerance, returnPopupsOnly, maxResultCount);
                        layerResults = new List<IdentifyLayerResult> { identifyResult }.AsReadOnly();
                    }
                    else if (target is GraphicsOverlay targetOverlay)
                    {
                        // identify features in the target layer, passing the tap point, tolerance, types to return, and max results
                        var identifyResult = await mapView.IdentifyGraphicsOverlayAsync(targetOverlay, tapScreenPoint, pixelTolerance, returnPopupsOnly, maxResultCount);
                        graphicsOverlayResults = new List<IdentifyGraphicsOverlayResult> { identifyResult }.AsReadOnly();
                    }
                    else if (target is ArcGISSublayer sublayer)
                    {
                        var layer = mapView?.Map?.AllLayers?.OfType<ArcGISMapImageLayer>()?.Where(l => l.Sublayers.Contains(sublayer))?.FirstOrDefault();

                        // identify features in the target layer, passing the tap point, tolerance, types to return, and max results
                        var topLevelIdentifyResult = await mapView.IdentifyLayerAsync(layer, tapScreenPoint, pixelTolerance, returnPopupsOnly, maxResultCount);
                        var sublayerIdentifyResult = topLevelIdentifyResult?.SublayerResults?.Where(r => r.LayerContent.Equals(sublayer)).FirstOrDefault();

                        layerResults = new List<IdentifyLayerResult> { sublayerIdentifyResult }.AsReadOnly();
                    }
                }
                catch
                {
                    // TODO: Alert user if error occured when trying to identify
                }

                OnIdentifyCompleted(layerResults, graphicsOverlayResults);
            }

            _isIdentifyInProgress = false;
        }

        /// <summary>
        /// Gets or sets the layer or overlay on which to perform identify operations
        /// </summary>
        public IPopupSource Target { get; set; }

        /// <summary>
        /// Find the GeoElement closest to the specified location
        /// </summary>
        /// <returns>The closest GeoElement to the input point</returns>
        private GeoElement FindNearestGeoElement(Geometry location, IReadOnlyList<GeoElement> geoElements)
        {
            if (geoElements == null || geoElements.Count == 0)
            {
                return null;
            }
            else if (geoElements.Count == 1)
            {
                return geoElements[0];
            }
            else
            {
                // Sort list of GeoElements by comparing the distance between them and the tapped screen location
                var sortableGeoElements = geoElements.ToList();
                sortableGeoElements.Sort((a, b) => GeometryEngine.Distance(location, a.Geometry).CompareTo(GeometryEngine.Distance(location, b.Geometry)));
                return sortableGeoElements[0];
            }
        }

        public event EventHandler<IdentifyEventArgs> IdentifyCompleted;

        private void OnIdentifyCompleted(IReadOnlyList<IdentifyLayerResult> layerResults,
            IReadOnlyList<IdentifyGraphicsOverlayResult> graphicsOverlayResults)
        {
            IdentifyCompleted?.Invoke(this, new IdentifyEventArgs(layerResults, graphicsOverlayResults));
        }
    }
}
