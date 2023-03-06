using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;


using Wsfly.ERP.Std.AppCode.Models;
using Wsfly.ERP.Std.AppCode.Base;
using System.Text.RegularExpressions;
using Wsfly.ERP.Std.Service.Dao;

namespace Wsfly.ERP.Std.Views.Parts
{
    /// <summary>
    /// ReportListUC.xaml 的交互逻辑
    /// </summary>
    public partial class PrintTemplateListUC : BaseUserControl
    {
        /// <summary>
        /// 表编号
        /// </summary>
        long _tableId = 0;
        /// <summary>
        /// 模版数据
        /// </summary>
        DataSet _templateData = null;
        /// <summary>
        /// 上级列表
        /// </summary>
        Home.ListUC _ucParent = null;

        /// <summary>
        /// 构造
        /// </summary>
        public PrintTemplateListUC(long tableId, DataSet ds, Home.ListUC ucParent)
        {
            _tableId = tableId;
            _templateData = ds;
            _ucParent = ucParent;

            InitializeComponent();

            this.Loaded += ReportListUC_Loaded;
        }
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReportListUC_Loaded(object sender, RoutedEventArgs e)
        {
            //加载数据
            LoadData();

            //点击事件
            this.btnChooseFile.Click += BtnChooseFile_Click;
            this.btnBuild.Click += BtnBuild_Click;

            this.btnClose.Click += BtnClose_Click;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            //移除控件
            (_ParentUC as Home.ListUC).gridMain.Children.Remove(this);
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            //显示等待
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {
                        TableName = "Sys_PrintTemplates",
                        Wheres = new List<Where>()
                        {
                            new Where() { CellName="TableId", CellValue=_tableId }
                        }
                    };
                    DataTable dt = SQLiteDao.GetTable(param);
                    if (dt == null || dt.Rows.Count <= 0)
                    {
                        HideLoading();
                        return;
                    }

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            PrintTemplateItemUC uc = new PrintTemplateItemUC(row, this);
                            uc.Margin = new Thickness(0, 0, 0, 5);
                            this.panelTemplates.Children.Add(uc);

                            //设置默认模版
                            bool isDefault = DataType.Bool(row["IsDefault"].ToString(), false);
                            if (isDefault)
                            {
                                _ucParent.SetDefaultPrintTemplate(row);
                            }
                        }

                        //隐藏等待
                        HideLoading();

                        return null;
                    }));
                }
                catch
                {
                    HideLoading();
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 创建模版
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBuild_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //中文界面
                //FastReport.Utils.Res.LoadLocale(Environment.CurrentDirectory + @"\FastReport.Net\Localization\Chinese (Simplified).frl");
                //创建打印报表
                FastReport.Report report = new FastReport.Report();
                //注册数据
                report.RegisterData(_templateData);
                //显示报表设计器
                //bool flag = report.Design();

                bool flag = false;

                #region 将打印模版保存到程序目录
                if (flag)
                {
                    //设置完成
                    string code = Core.Handler.FileHandler.ReadFile(report.FileName);
                    string fileName = System.IO.Path.GetFileName(report.FileName);

                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        //显示加载
                        ShowLoading(gridMain);

                        System.Threading.Thread threadSaveToServer = new System.Threading.Thread(delegate ()
                        {
                            try
                            {
                                SQLParam insertParam = new SQLParam()
                                {
                                    TableName = "Sys_PrintTemplates",
                                    OpreateCells = new List<KeyValue>()
                                    {
                                        new KeyValue("UserId", AppGlobal.UserInfo.UserId),
                                        new KeyValue("TableId", _tableId),
                                        new KeyValue("TemplateName", fileName),
                                        new KeyValue("TemplateCode", code),
                                        new KeyValue("Version", 1),
                                        new KeyValue("IsDefault", false),
                                        new KeyValue("CreateDate", DateTime.Now),
                                    }
                                };

                                //更新
                                long id = SQLiteDao.Insert(insertParam);

                                DataRow row = null;
                                if (id > 0)
                                {
                                    row = SQLiteDao.GetTableRow(id, "Sys_PrintTemplates");
                                }

                                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                                {
                                    if (row != null)
                                    {
                                        PrintTemplateItemUC uc = new PrintTemplateItemUC(row, this);
                                        uc.Margin = new Thickness(0, 0, 0, 5);
                                        this.panelTemplates.Children.Add(uc);

                                        AppAlert.FormTips(gridMain, "添加模版成功！", AppCode.Enums.FormTipsType.Right);
                                    }
                                    else
                                    {
                                        AppAlert.FormTips(gridMain, "添加模版失败！", AppCode.Enums.FormTipsType.Info);
                                    }

                                    //隐藏提示
                                    HideLoading();

                                    return null;
                                }));
                            }
                            catch
                            {
                                //隐藏等待
                                HideLoading();

                                this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                                {
                                    AppAlert.FormTips(gridMain, "添加模版失败！", AppCode.Enums.FormTipsType.Info);
                                    return null;
                                }));
                            }
                        });
                        threadSaveToServer.IsBackground = true;
                        threadSaveToServer.Start();
                    }
                }
                #endregion

                //释放资源
                report.Dispose();
            }
            catch (Exception ex)
            {
                //日志
                ErrorHandler.AddException(ex, "打印模版设置异常，加载设计报表控件失败！");
                //提示
                AppAlert.FormTips(gridMain, "加载设计报表控件失败！", AppCode.Enums.FormTipsType.Info);
            }
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnChooseFile_Click(object sender, RoutedEventArgs e)
        {
            //选择文件窗口
            FileManagerUC uc = new FileManagerUC();

            //弹出窗口
            Views.Components.PopWindow win = new Components.PopWindow(uc, "选择打印模版");

            //上级窗口
            uc._ParentWindow = win;

            //窗口属性
            win._TargetObj = sender;
            win._ShowMaxBtn = true;
            win.CallBack_Event += WinChooseFile_CallBack_Event;
            win.ShowDialog();
        }

        /// <summary>
        /// 选择文件回传
        /// </summary>
        /// <param name="win"></param>
        /// <param name="param"></param>
        private void WinChooseFile_CallBack_Event(Components.PopWindow win, object param)
        {
            //选择的文件
            FileItemInfo file = param as FileItemInfo;
            if (file == null) return;

            if (!file.FileExt.ToLower().Equals(".frx"))
            {
                AppAlert.FormTips((win._UIElement as FileManagerUC).gridMain, "请选择.frx的打印模版文件！", AppCode.Enums.FormTipsType.Info);
                return;
            }

            //关闭窗口
            win.Close();

            string filePath = AppGlobal.GetUploadFilePath(file.FilePath);
            if (!System.IO.File.Exists(filePath))
            {
                AppAlert.FormTips((win._UIElement as FileManagerUC).gridMain, "模版文件不存在！", AppCode.Enums.FormTipsType.Info);
                return;
            }

            string code = Core.Handler.FileHandler.ReadFile(filePath);

            //显示等待
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                SQLParam insertParam = new SQLParam()
                {
                    TableName = "Sys_PrintTemplates",
                    OpreateCells = new List<KeyValue>()
                    {
                        new KeyValue("UserId", AppGlobal.UserInfo.UserId),
                        new KeyValue("TableId", _tableId),
                        new KeyValue("TemplateName", file.FileName),
                        //new KeyValue("TemplatePath", file.FilePath),
                        new KeyValue("TemplateCode", code),
                        new KeyValue("Version", 1),
                        new KeyValue("IsDefault", false),
                        new KeyValue("CreateDate", DateTime.Now),
                    }
                };

                try
                {
                    long id = SQLiteDao.Insert(insertParam);

                    DataRow row = null;
                    if (id > 0)
                    {
                        row = SQLiteDao.GetTableRow(id, "Sys_PrintTemplates");
                    }

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (row != null)
                        {
                            PrintTemplateItemUC uc = new PrintTemplateItemUC(row, this);
                            uc.Margin = new Thickness(0, 0, 0, 5);
                            this.panelTemplates.Children.Add(uc);

                            AppAlert.FormTips(gridMain, "添加模版成功！", AppCode.Enums.FormTipsType.Right);
                        }
                        else
                        {
                            AppAlert.FormTips(gridMain, "添加模版失败！", AppCode.Enums.FormTipsType.Info);
                        }

                        //隐藏提示
                        HideLoading();

                        return null;
                    }));
                }
                catch
                {
                    //隐藏提示
                    HideLoading();

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "添加模版失败！", AppCode.Enums.FormTipsType.Info);
                        return null;
                    }));
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 设为默认或取消默认
        /// </summary>
        /// <param name="row"></param>
        /// <param name="isDefault"></param>
        public void SetDeafult(DataRow row, bool isDefault)
        {
            long id = DataType.Long(row["Id"], 0);
            if (id <= 0) return;

            if (isDefault)
            {
                //设置默认模版
                _ucParent.SetDefaultPrintTemplate(row);
            }
            else
            {
                //取消默认模版
                _ucParent.SetDefaultPrintTemplate(null);
            }

            //显示等待
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    //将原默认取消
                    SQLParam param = new SQLParam()
                    {
                        TableName = "Sys_PrintTemplates",
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("IsDefault", 0)
                        },
                        Wheres = new List<Where>()
                        {
                            new Where() { CellName="UserId", CellValue=AppGlobal.UserInfo.UserId },
                            new Where() { CellName="TableId", CellValue=_tableId },
                            new Where() { CellName="IsDefault", CellValue=1 }
                        }
                    };
                    bool flag = SQLiteDao.Update(param);

                    //更新默认
                    param = new SQLParam()
                    {
                        Id = id,
                        TableName = "Sys_PrintTemplates",
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("IsDefault", isDefault)
                        }
                    };
                    flag = SQLiteDao.Update(param);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (flag)
                        {
                            AppAlert.FormTips(gridMain, "设置成功！", AppCode.Enums.FormTipsType.Right);

                            foreach (PrintTemplateItemUC uc in this.panelTemplates.Children)
                            {
                                if (uc._rowTemplate == row)
                                {
                                    uc.SetDefault(isDefault);
                                }
                                else
                                {
                                    uc.SetDefault(false);
                                }
                            }
                        }
                        else
                        {
                            AppAlert.FormTips(gridMain, "设置失败！", AppCode.Enums.FormTipsType.Info);
                        }

                        //隐藏等待
                        HideLoading();
                        return null;
                    }));
                }
                catch
                {
                    HideLoading();
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "设置失败！", AppCode.Enums.FormTipsType.Info);
                        return null;
                    }));
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 删除打印模版
        /// </summary>
        /// <param name="row"></param>
        /// <param name="uc"></param>
        public void DeleteTemplate(DataRow row, PrintTemplateItemUC uc)
        {
            long id = DataType.Long(row["Id"], 0);
            if (id <= 0) return;

            //是否确定删除
            bool? isDeleteConfirm = AppAlert.Alert("是否确定删除打印模版？", "删除提示", AppCode.Enums.AlertWindowButton.OkCancel);
            if (!isDeleteConfirm.HasValue || !isDeleteConfirm.Value) return;

            //显示等待
            ShowLoading(gridMain);

            System.Threading.Thread thread = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {
                        TableName = "Sys_PrintTemplates",
                        Id = id,
                    };

                    //删除
                    bool flag = SQLiteDao.Delete(param);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        if (flag)
                        {
                            AppAlert.FormTips(gridMain, "删除成功！", AppCode.Enums.FormTipsType.Right);
                            this.panelTemplates.Children.Remove(uc);
                        }
                        else
                        {
                            AppAlert.FormTips(gridMain, "删除失败！", AppCode.Enums.FormTipsType.Info);
                        }

                        HideLoading();

                        return null;
                    }));
                }
                catch
                {
                    HideLoading();
                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "删除失败！", AppCode.Enums.FormTipsType.Info);
                        return null;
                    }));
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 编辑打印模版
        /// </summary>
        /// <param name="row"></param>
        /// <param name="uc"></param>
        public void EditTemplate(DataRow row, PrintTemplateItemUC uc)
        {
            long id = DataType.Long(row["Id"], 0);
            if (id <= 0) return;

            try
            {
                //报表文件
                string reportCode = row["TemplateCode"].ToString();
                long tableId = DataType.Long(row["TableId"], 1);
                int version = DataType.Int(row["Version"], 1);
                int nextVersion = version + 1;

                MZ.XMLEditorUC ucXMLEditor = new MZ.XMLEditorUC(reportCode);
                Components.PageWindow pageWindow = new Components.PageWindow(ucXMLEditor, "打印模版编辑");
                ucXMLEditor._ParentWindow = pageWindow;
                pageWindow.CallBack_Event += new Components.PageWindowCallBackDelegate((Components.PageWindow win, object param) =>
                {
                    string code = param.ToString();

                    //显示加载
                    ShowLoading(gridMain);

                    SaveTemplateCode(row, uc, id, nextVersion, code);

                    win.Close();
                });
                pageWindow.ShowDialog();

                return;

                //========================================================================================

                //中文界面
                FastReport.Utils.Res.LoadLocale(AppDomain.CurrentDomain.BaseDirectory + @"FastReport.Net\Localization\Chinese (Simplified).frl");
                //创建打印报表
                FastReport.Report report = new FastReport.Report();
                if (!string.IsNullOrWhiteSpace(reportCode))
                {
                    //加载报表模版
                    report.LoadFromString(reportCode);
                }
                //注册数据
                report.RegisterData(_templateData);

                //显示报表设计器
                //bool flag = report.Design();

                bool flag = false;

                #region 读取模版并更新
                if (flag)
                {
                    string code = Core.Handler.FileHandler.ReadFile(report.FileName, "UTF-8");

                    //是否存在文件
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        //显示加载
                        ShowLoading(gridMain);

                        SaveTemplateCode(row, uc, id, nextVersion, code);
                    }
                }
                #endregion

                //释放资源
                report.Dispose();
            }
            catch (Exception ex)
            {
                //日志
                ErrorHandler.AddException(ex, "打印模版设置异常，加载设计报表控件失败！");
                //提示
                AppAlert.FormTips(gridMain, "加载设计报表控件失败！", AppCode.Enums.FormTipsType.Info);
            }
        }
        /// <summary>
        /// 保存模版代码
        /// </summary>
        /// <param name="row"></param>
        /// <param name="uc"></param>
        /// <param name="id"></param>
        /// <param name="nextVersion"></param>
        /// <param name="code"></param>
        private void SaveTemplateCode(DataRow row, PrintTemplateItemUC uc, long id, int nextVersion, string code)
        {
            System.Threading.Thread threadSaveToServer = new System.Threading.Thread(delegate ()
            {
                try
                {
                    SQLParam param = new SQLParam()
                    {
                        Id = id,
                        TableName = "Sys_PrintTemplates",
                        OpreateCells = new List<KeyValue>()
                        {
                            new KeyValue("TemplateCode", code),
                            new KeyValue("Version", nextVersion)
                        }
                    };

                    //更新
                    bool flagSaveTemplate = SQLiteDao.Update(param);

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        uc.SetTemplateCode = code;

                        //隐藏等待
                        HideLoading();

                        if (flagSaveTemplate)
                        {
                            row["Version"] = nextVersion;
                            AppAlert.FormTips(gridMain, "打印模版保存成功！", AppCode.Enums.FormTipsType.Right);
                        }
                        else
                        {
                            AppAlert.FormTips(gridMain, "打印模版保存失败！", AppCode.Enums.FormTipsType.Info);
                        }
                        return null;
                    }));
                }
                catch
                {
                    //隐藏等待
                    HideLoading();

                    this.Dispatcher.Invoke(new FlushClientBaseDelegate(delegate ()
                    {
                        AppAlert.FormTips(gridMain, "打印模版保存失败！", AppCode.Enums.FormTipsType.Info);
                        return null;
                    }));
                }
            });
            threadSaveToServer.IsBackground = true;
            threadSaveToServer.Start();
        }
    }
}
