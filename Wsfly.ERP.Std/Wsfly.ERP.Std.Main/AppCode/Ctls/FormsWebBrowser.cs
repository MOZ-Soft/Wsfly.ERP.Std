using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace Wsfly.ERP.Std.AppCode.Ctls
{
    public class FormsWebBrowser
    {
        Window _owner;
        FrameworkElement _placementTarget;
        Form _form;
        WebBrowser _wb = new WebBrowser();

        public WebBrowser WebBrowser { get { return _wb; } }

        public FormsWebBrowser(FrameworkElement placementTarget)
        {
            _placementTarget = placementTarget;
            Window owner = Window.GetWindow(placementTarget);
            Debug.Assert(owner != null);
            _owner = owner;

            _form = new Form();
            _form.Opacity = owner.Opacity;
            _form.ShowInTaskbar = false;
            _form.FormBorderStyle = FormBorderStyle.None;
            _wb.Dock = DockStyle.Fill;
            _form.Controls.Add(_wb);

            owner.LocationChanged += delegate { OnSizeLocationChanged(); };
            _placementTarget.SizeChanged += delegate { OnSizeLocationChanged(); };

            if (owner.IsVisible)
                InitialShow();
            else
                owner.SourceInitialized += delegate
                {
                    InitialShow();
                };

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(UIElement.OpacityProperty, typeof(Window));
            dpd.AddValueChanged(owner, delegate { _form.Opacity = _owner.Opacity; });

            _form.FormClosing += delegate { _owner.Close(); };
        }

        void InitialShow()
        {
            NativeWindow owner = new NativeWindow();
            owner.AssignHandle(((HwndSource)HwndSource.FromVisual(_owner)).Handle);
            _form.Show(owner);
            owner.ReleaseHandle();
        }

        DispatcherOperation _repositionCallback;

        void OnSizeLocationChanged()
        {
            if (_repositionCallback == null)
                _repositionCallback = _owner.Dispatcher.BeginInvoke(new Action(Reposition), DispatcherPriority.Input);
        }

        void Reposition()
        {
            _repositionCallback = null;

            Point offset = _placementTarget.TranslatePoint(new Point(), _owner);
            Point size = new Point(_placementTarget.ActualWidth, _placementTarget.ActualHeight);
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual(_owner);
            CompositionTarget ct = hwndSource.CompositionTarget;
            offset = ct.TransformToDevice.Transform(offset);
            size = ct.TransformToDevice.Transform(size);

            Win32.POINT screenLocation = new Win32.POINT(offset);
            Win32.ClientToScreen(hwndSource.Handle, ref screenLocation);
            Win32.POINT screenSize = new Win32.POINT(size);

            Win32.MoveWindow(_form.Handle, screenLocation.X, screenLocation.Y, screenSize.X, screenSize.Y, true);
        }
    }

    static class Win32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
            public POINT(Point pt)
            {
                X = Convert.ToInt32(pt.X);
                Y = Convert.ToInt32(pt.Y);
            }
        };

        [DllImport("user32.dll")]
        internal static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    }
}
