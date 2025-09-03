using System;

namespace ChronoPos.Desktop.Interfaces
{
    #region Enums

    /// <summary>
    /// Available zoom levels for the application
    /// </summary>
    public enum ZoomLevel
    {
        Zoom50 = 50,
        Zoom60 = 60,
        Zoom70 = 70,
        Zoom80 = 80,
        Zoom90 = 90,
        Zoom100 = 100,
        Zoom110 = 110,
        Zoom120 = 120,
        Zoom130 = 130,
        Zoom140 = 140,
        Zoom150 = 150
    }

    #endregion

    /// <summary>
    /// Service interface for managing application zoom functionality
    /// </summary>
    public interface IZoomService
    {
        #region Events

        /// <summary>
        /// Event raised when zoom level changes
        /// </summary>
        event Action<ZoomLevel> ZoomChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current zoom level
        /// </summary>
        ZoomLevel CurrentZoomLevel { get; }

        /// <summary>
        /// Gets the current zoom scale factor (e.g., 1.0 for 100%)
        /// </summary>
        double CurrentZoomScale { get; }

        /// <summary>
        /// Gets the current zoom percentage (e.g., 100 for 100%)
        /// </summary>
        int CurrentZoomPercentage { get; }

        /// <summary>
        /// Gets whether the current zoom level can be increased
        /// </summary>
        bool CanZoomIn { get; }

        /// <summary>
        /// Gets whether the current zoom level can be decreased
        /// </summary>
        bool CanZoomOut { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Changes the zoom level to a specific level
        /// </summary>
        /// <param name="zoomLevel">The target zoom level</param>
        void ChangeZoomLevel(ZoomLevel zoomLevel);

        /// <summary>
        /// Changes the zoom level to a specific percentage
        /// </summary>
        /// <param name="percentage">The target zoom percentage (50-150)</param>
        void ChangeZoomLevel(int percentage);

        /// <summary>
        /// Increases the zoom level by one step
        /// </summary>
        void ZoomIn();

        /// <summary>
        /// Decreases the zoom level by one step
        /// </summary>
        void ZoomOut();

        /// <summary>
        /// Resets zoom to 100%
        /// </summary>
        void ResetZoom();

        /// <summary>
        /// Loads the zoom level from application settings
        /// </summary>
        void LoadZoomFromSettings();

        /// <summary>
        /// Saves the current zoom level to settings
        /// </summary>
        void SaveZoomToSettings();

        /// <summary>
        /// Forces a refresh of the application UI
        /// </summary>
        void RefreshApplicationUI();

        #endregion
    }
}
