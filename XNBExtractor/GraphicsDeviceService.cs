#region File Description
//-----------------------------------------------------------------------------
// GraphicsDeviceService.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
#endregion

// The IGraphicsDeviceService interface requires a DeviceCreated event, but we
// always just create the device inside our constructor, so we have no place to
// raise that event. The C# compiler warns us that the event is never used, but
// we don't care so we just disable this warning.
#pragma warning disable 67

namespace XNBExtractor
{
    /// <summary>
    /// Helper class responsible for creating and managing the GraphicsDevice.
    /// All GraphicsDeviceControl instances share the same GraphicsDeviceService,
    /// so even though there can be many controls, there will only ever be a single
    /// underlying GraphicsDevice. This implements the standard IGraphicsDeviceService
    /// interface, which provides notification events for when the device is reset
    /// or disposed.
    /// </summary>
    class GraphicsDeviceService : IGraphicsDeviceService
    {
        #region Fields

        // Singleton device service instance.
        static GraphicsDeviceService singletonInstance;


        // Keep track of how many controls are sharing the singletonInstance.
        static int referenceCount;


        /// <summary>
        /// Gets the current graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get => graphicsDevice;
            private set => graphicsDevice = value;
        }


        public static GraphicsProfile GraphicsProfile
        {
            get => graphicsProfile;
            set
            {
                if (graphicsProfile != value)
                {
                    graphicsProfile = value;

                    if (graphicsDevice != null && graphicsDevice.GraphicsProfile != value)
                    {
                        if (!graphicsDevice.IsDisposed)
                        {
                            graphicsDevice.Dispose();
                        }

                        graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter,
                                                            graphicsProfile,
                                                            parameters);
                    }
                }
            }
        }

        #endregion


        /// <summary>
        /// Constructor is private, because this is a singleton class:
        /// client controls should use the public AddRef method instead.
        /// </summary>
        GraphicsDeviceService(IntPtr windowHandle, int width, int height)
        {
            parameters = new PresentationParameters
            {
                BackBufferWidth = Math.Max(width, 1),
                BackBufferHeight = Math.Max(height, 1),
                BackBufferFormat = SurfaceFormat.Color,
                DepthStencilFormat = DepthFormat.Depth24,
                DeviceWindowHandle = windowHandle,
                PresentationInterval = PresentInterval.Immediate,
                IsFullScreen = false
            };

            GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter,
                                                GraphicsProfile,
                                                parameters);
        }


        /// <summary>
        /// Gets a reference to the singleton instance.
        /// </summary>
        public static GraphicsDeviceService AddRef(IntPtr windowHandle,
                                                   int width, int height)
        {
            // Increment the "how many controls sharing the device" reference count.
            if (Interlocked.Increment(ref referenceCount) == 1)
            {
                // If this is the first control to start using the
                // device, we must create the singleton instance.
                singletonInstance = new GraphicsDeviceService(windowHandle,
                                                              width, height);
            }

            return singletonInstance;
        }


        /// <summary>
        /// Releases a reference to the singleton instance.
        /// </summary>
        public void Release(bool disposing)
        {
            // Decrement the "how many controls sharing the device" reference count.
            if (Interlocked.Decrement(ref referenceCount) == 0)
            {
                // If this is the last control to finish using the
                // device, we should dispose the singleton instance.
                if (disposing)
                {
                    DeviceDisposing?.Invoke(this, EventArgs.Empty);
                    GraphicsDevice.Dispose();
                }

                GraphicsDevice = null;
            }
        }


        /// <summary>
        /// Resets the graphics device to whichever is bigger out of the specified
        /// resolution or its current size. This behavior means the device will
        /// demand-grow to the largest of all its GraphicsDeviceControl clients.
        /// </summary>
        public void ResetDevice(int width, int height)
        {
            DeviceResetting?.Invoke(this, EventArgs.Empty);

            parameters.BackBufferWidth = Math.Max(parameters.BackBufferWidth, width);
            parameters.BackBufferHeight = Math.Max(parameters.BackBufferHeight, height);

            GraphicsDevice.Reset(parameters);

            DeviceReset?.Invoke(this, EventArgs.Empty);
        }


        // Store the current device settings.
        private static PresentationParameters parameters;
        private static GraphicsProfile graphicsProfile;
        private static GraphicsDevice graphicsDevice;


        // IGraphicsDeviceService events.
        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
    }
}
