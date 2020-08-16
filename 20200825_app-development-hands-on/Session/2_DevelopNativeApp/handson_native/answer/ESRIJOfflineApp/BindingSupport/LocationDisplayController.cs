using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Windows;

namespace ESRIJOfflineApp.BindingSupport
{
    public class LocationDisplayController : DependencyObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserLocationController"/> class.
        /// </summary>
        public LocationDisplayController() { }

        /// <summary>
        /// MapView setter
        /// </summary>
        internal void SetMapView(MapView mapView)
        {
            MapView = mapView;
        }

        private WeakReference<MapView> _mapViewWeakRef;

        /// <summary>
        /// Gets or sets the MapView
        /// </summary>
        private MapView MapView
        {
            get
            {
                MapView mapView = null;
                _mapViewWeakRef?.TryGetTarget(out mapView);
                return mapView;
            }
            set
            {
                if (_mapViewWeakRef == null)
                    _mapViewWeakRef = new WeakReference<MapView>(value);
                else
                    _mapViewWeakRef.SetTarget(value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="DataSourceProperty"/> property
        /// </summary>
        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(nameof(DataSource), typeof(LocationDataSource), typeof(LocationDisplayController),
            new PropertyMetadata(null, OnLocationDataSourceChanged));

        /// <summary>
        /// Invoked when the DataSourceProperty changes
        /// </summary>
        private static void OnLocationDataSourceChanged(DependencyObject dependency, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is LocationDataSource && (dependency as LocationDisplayController).MapView != null)
            {
                var locationDisplay = (dependency as LocationDisplayController).MapView.LocationDisplay;
                locationDisplay.DataSource = (LocationDataSource)args.NewValue;
            }
        }

        /// <summary>
        /// Gets or sets the DataSource property
        /// </summary>
        public LocationDataSource DataSource
        {
            get { return MapView?.LocationDisplay.DataSource; }
            set { SetValue(DataSourceProperty, value); }
        }
    }
}
