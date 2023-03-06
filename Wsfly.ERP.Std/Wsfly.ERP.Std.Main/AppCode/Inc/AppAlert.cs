using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wsfly.ERP.Std.Views.Components;
using Wsfly.ERP.Std.AppCode.Enums;

/// <summary>
/// 提示
/// </summary>
public class AppAlert
{
    /// <summary>
    /// 提示
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <param name="btns"></param>
    /// <param name="showClose"></param>
    public static bool? Alert(
        string message,
        string title = "温馨提示",
        AlertWindowButton btns = AlertWindowButton.Ok,
        bool showClose = true,
        System.Windows.HorizontalAlignment hAlign = System.Windows.HorizontalAlignment.Center,
        System.Windows.VerticalAlignment vAlign = System.Windows.VerticalAlignment.Center)
    {
        AlertWindow alertWin = new AlertWindow();
        alertWin.Topmost = true;
        alertWin.TextHorizontalAlignment(hAlign);
        alertWin.TextVerticalAlignment(vAlign);
        return alertWin.Alert(message, title, btns, showClose);
    }
    /// <summary>
    /// 提示
    /// </summary>
    /// <param name="element"></param>
    /// <param name="title"></param>
    /// <param name="btns"></param>
    /// <param name="showClose"></param>
    public static bool? Alert(System.Windows.UIElement element, string title = "温馨提示", AlertWindowButton btns = AlertWindowButton.Ok, bool showClose = true)
    {
        AlertWindow alertWin = new AlertWindow();
        alertWin.Topmost = true;
        return alertWin.Alert(element, title, btns, showClose);
    }
    /// <summary>
    /// 提示
    /// </summary>
    /// <param name="element"></param>
    /// <param name="title"></param>
    /// <param name="btns"></param>
    /// <param name="showClose"></param>
    public static AlertWindow Alert(string message, string title,
        System.Windows.HorizontalAlignment hAlign,
        System.Windows.VerticalAlignment vAlign,
        double width = 300,
        double height = 200)
    {
        AlertWindow alertWin = new AlertWindow();
        alertWin.Topmost = true;
        alertWin.TextHorizontalAlignment(hAlign);
        alertWin.TextVerticalAlignment(vAlign);
        alertWin.Width = width;
        alertWin.Height = height;
        alertWin.Alert(message, title);
        return alertWin;
    }


    /// <summary>
    /// 右下角提示
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="title"></param>
    /// <param name="autoClose"></param>
    /// <param name="showClose"></param>
    public static TipsWindow Tips(string msg, string title = "温馨提示", bool autoClose = false, bool showClose = true)
    {
        TipsWindow tipsWin = new TipsWindow();
        tipsWin.Topmost = true;
        tipsWin.ShowMessage(msg, title, autoClose, showClose);
        return tipsWin;
    }
    /// <summary>
    /// 右下角提示
    /// </summary>
    /// <param name="element"></param>
    /// <param name="title"></param>
    /// <param name="autoClose"></param>
    /// <param name="showClose"></param>
    public static TipsWindow Tips(System.Windows.UIElement element, string title = "温馨提示", bool autoClose = false, bool showClose = true)
    {
        TipsWindow tipsWin = new TipsWindow();
        tipsWin.Topmost = true;
        tipsWin.ShowContent(element, title, autoClose, showClose);
        return tipsWin;
    }

    /// <summary>
    /// 提示框
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="autoClose"></param>
    /// <param name="seconds"></param>
    public static FormTipsView FormTips(System.Windows.Controls.Grid grid, string msg, FormTipsType type = FormTipsType.Info, bool autoClose = true, int seconds = 3, bool isShowFullBG = false)
    {
        if (!autoClose) seconds = 0;

        FormTipsView view = new FormTipsView(msg, type, seconds, isShowFullBG);

        grid.Children.Add(view);

        if (grid.RowDefinitions.Count > 0)
            System.Windows.Controls.Grid.SetRowSpan(view, grid.RowDefinitions.Count);
        if (grid.ColumnDefinitions.Count > 0)
            System.Windows.Controls.Grid.SetColumnSpan(view, grid.ColumnDefinitions.Count);

        int _timerAutoCloseIndex = 1;

        if (autoClose)
        {
            //定时自动关闭
            System.Windows.Threading.DispatcherTimer _timerAutoColose = new System.Windows.Threading.DispatcherTimer();
            _timerAutoColose.Interval = new TimeSpan(0, 0, 1);
            _timerAutoColose.Tick += new EventHandler(delegate (object sender, EventArgs e)
            {
                _timerAutoCloseIndex++;

                if (_timerAutoCloseIndex >= seconds)
                {
                    grid.Children.Remove(view);
                    _timerAutoColose.Stop();
                    _timerAutoColose = null;
                }
            });
            _timerAutoColose.Start();
        }

        return view;
    }
}

