using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Collections;
using Microsoft.Win32;
using System.IO;
using System.Runtime.Remoting.Messaging;
using MyTools.DataDic.Utils;
using System.Net;
using MyTools.Update;
using System.Xml;

namespace MyTools.DataDic
{
    public partial class DataDicForm : Form
    {
        /// <summary>
        /// treeview数据源
        /// </summary>
        private List<PhysicalDiagramInfo> listPhysicalDiagramInfo = null;

        private List<TableInfo> list = null;

        private DataTable dt = new DataTable();

        /// <summary>
        /// 程序设置信息
        /// </summary>
        private static Properties.Settings setting = Properties.Settings.Default;
        private static string softVerion ="";



        public DataDicForm()
        {
            InitializeComponent();
            dt.Columns.Add("Type");
            dt.Columns.Add("Message");
            dt.Columns.Add("Time");
        }

        /// <summary>
        /// 初始化表信息
        /// </summary>
        private void InitTableInfo()
        {
            try
            {
                string path = txtPath.Text.Trim();
                IReader reader = ReaderFactory.GetReader(path);
                list = reader.GetTableInfo();
                listPhysicalDiagramInfo = reader.GetPDInfo();
                Clear(false);
            }
            catch (Exception ex)
            {
                Clear(true);
                BindData(new Result(Level.Execption, ex.Message));
            }
        }

        /// <summary>
        /// 模版信息改变时执行
        /// </summary>
        private void Clear(bool isExecption)
        {
            //清空提示信息数据源
            dt.Clear();
            //清空过滤表名
            textTableList.Text = "";
            //清空提示
            dataGridView1.DataSource = dt;
            if (isExecption)
            {
                listPhysicalDiagramInfo = null;
                list = null;
            }
        }


        /// <summary>
        /// 生成SQL脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGenSQL_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtPath.Text.Trim()))
                {
                    MessageBox.Show("请选择模版文件路径！");
                    return;
                }
                if (string.IsNullOrEmpty(txtOutPutPath.Text.Trim()))
                {
                    MessageBox.Show("请输入脚本输出路径！");
                    return;
                }
                //以修改模式校验数据库连接是否正确
                if (radioUpdate.Checked || radioAuto.Checked)
                {
                    string strCon = Properties.Settings.Default.ConnectionString;
                    if (String.IsNullOrEmpty(strCon))
                    {
                        MessageBox.Show("请先进行数据库连接配置！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    try
                    {
                        Common.IsCorrectConnection(strCon);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                if (!Directory.Exists(txtOutPutPath.Text.Trim()))
                {
                    DialogResult dr = MessageBox.Show("脚本输出路径不存在，是否创建！", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (dr != DialogResult.OK)
                    {
                        return;
                    }
                }
                //杜冬军2014-05-06 修改  生成路径改变但是没有记住的BUG
                Directory.CreateDirectory(txtOutPutPath.Text.Trim());
                Properties.Settings.Default.OutPutPath = txtOutPutPath.Text;
                setting.Save();
                setting.Upgrade();

                //让主线程去访问设置提示信息
                if (mBackgroundWorker.IsBusy)
                {
                    MessageBox.Show("当前进程正在生成脚本，请等待本次操作完成！");
                    return;
                }
                dt.Clear();
                dataGridView1.DataSource = dt;
                mBackgroundWorker.RunWorkerAsync(list);
            }
            catch (Exception ex)
            {
                BindData(new Result(Level.Execption, ex.Message));
            }
        }

        /// <summary>
        /// 生成表的SQL
        /// </summary>
        private Result GenSQL(TableInfo t)
        {

            Hashtable param = null;

            string tPath = GetTemplatePath(t);
            try
            {
                param = new Hashtable();
                param.Add("T", t);
                param.Add("V", softVerion);
                FileGen.GetFile(tPath, param, txtOutPutPath.Text.Trim() + @"\" + t.Code + ".sql");
                return new Result(Level.Success, t.Code);
            }
            catch (Exception ex)
            {
                return new Result(Level.Execption, ex.Message);
            }
        }

        /// <summary>
        /// 获取生成SQL的模版文件路径
        /// </summary>
        /// <returns>文件路径</returns>
        private string GetTemplatePath(TableInfo t)
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory + @"Common\";
            if (t.IsUpdate)
            {
                strPath = strPath + "UpdateDic.vm";
            }
            else
            {
                strPath = strPath + "DataDic.vm";
            }
            return strPath;
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "模版文件(*.pdm,*.xls,*.xlsx,*.docx)|*.pdm;*.xls;*.xlsx;*.docx";
            openFileDialog1.FileName = "";
            openFileDialog1.Title = "选择模版文件";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = openFileDialog1.FileName;
            }
        }

        /// <summary>
        /// 选择表名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSerach_Click(object sender, EventArgs e)
        {
            if (listPhysicalDiagramInfo != null)
            {
                Form serachForm = new SerachForm(listPhysicalDiagramInfo);
                serachForm.ShowDialog();
                List<string> list = (List<string>)serachForm.Tag;
                if (list != null)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string s in list)
                    {
                        sb.Append(s + ",");
                    }
                    textTableList.Text = sb.ToString().Substring(0, sb.ToString().Length - 1);
                }
                serachForm.Dispose();
            }
            else
            {
                MessageBox.Show("请选择模版文件！");
            }
        }

        /// <summary>
        /// pdm文件路径改变监听事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPath_TextChanged(object sender, EventArgs e)
        {
            InitTableInfo();
        }

        private void DataDicForm_Load(object sender, EventArgs e)
        {
            softVerion = setting.软件版本;
            if (string.IsNullOrEmpty(Properties.Settings.Default.OutPutPath))
            {
                RegistryKey folders = OpenRegistryPath(Registry.CurrentUser, @"/software/microsoft/windows/currentversion/explorer/shell folders");
                // Windows用户桌面路径  
                string desktopPath = folders.GetValue("Desktop").ToString();
                txtOutPutPath.Text = desktopPath + @"\SQL脚本输出";
                Properties.Settings.Default.OutPutPath = txtOutPutPath.Text;
                setting.Save();
                setting.Upgrade();
            }
            else
            {
                txtOutPutPath.Text = Properties.Settings.Default.OutPutPath;
            }

        }

        private RegistryKey OpenRegistryPath(RegistryKey root, string s)
        {
            s = s.Remove(0, 1) + @"/";
            while (s.IndexOf(@"/") != -1)
            {
                root = root.OpenSubKey(s.Substring(0, s.IndexOf(@"/")));
                s = s.Remove(0, s.IndexOf(@"/") + 1);
            }
            return root;
        }

        /// <summary>
        /// 绑定消息提示的数据源
        /// </summary>
        /// <param name="message">消息</param>
        private void BindData(Result r)
        {
            if (r != null)
            {
                DataRow dr = dt.NewRow();
                dr["Type"] = r.Type;
                dr["Message"] = r.Message;
                dr["Time"] = DateTime.Now.ToString();
                dt.Rows.Add(dr);
                DataView dv = dt.DefaultView;
                dv.Sort = "Type DESC,Time DESC";
                DataTable dt2 = dv.ToTable();
                dataGridView1.DataSource = dt2;
            }
        }

        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            DataGridViewRow dgr = dataGridView1.Rows[e.RowIndex];
            try
            {
                if (dgr.Cells["Type"].Value != null)
                {
                    if (dgr.Cells["Type"].Value.ToString() == Result.GetCLevel(Level.Execption))
                    {
                        dgr.DefaultCellStyle.ForeColor = Color.Red;
                    }
                    else if (dgr.Cells["Type"].Value.ToString() == Result.GetCLevel(Level.System))
                    {
                        dgr.DefaultCellStyle.ForeColor = Color.Blue;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        /// <summary>
        /// 生成SQL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<TableInfo> list = (List<TableInfo>)e.Argument;
            BackgroundWorker bw = (BackgroundWorker)sender;
            //生成SQL开始时间
            DateTime dtStart = DateTime.Now;

            //异常数
            int iExecption = 0;
            //操作表总数
            int iTotal = 0;
            Result r = null;
            //需要生成的表信息集合
            List<TableInfo> listGen = new List<TableInfo>();
            if (string.IsNullOrEmpty(textTableList.Text.Trim()))
            {
                listGen = list;
            }
            else
            {
                string[] arrTable = textTableList.Text.Split(new char[] { ',' });
                IEnumerable<TableInfo> query = from a in list
                                               join b in arrTable
                                               on a.Code equals b
                                               select a;
                listGen = query.ToList<TableInfo>();
            }
            //修改模式对比出变化的列信息
            if (!CompareTable(listGen))
            {
                return;
            }
            foreach (TableInfo t in listGen)
            {
                //没有取消后台操作
                if (!bw.CancellationPending)
                {
                    r = GenSQL(t);
                    iTotal++;
                    if (r.RLevel == Level.Execption)
                    {
                        iExecption++;
                    }
                    bw.ReportProgress(0, r);
                }
            }
            //生成SQL结束时间
            DateTime dtEnd = DateTime.Now;
            string sec = (dtEnd - dtStart).TotalSeconds.ToString("f");
            if (iExecption == 0)
            {
                bw.ReportProgress(0, new Result(Level.System, "本次共操作" + iTotal + "张表,全部生成成功,共耗时" + sec + "秒！"));
            }
            else
            {
                bw.ReportProgress(0, new Result(Level.System, "本次共操作" + iTotal + "张表," + (iTotal - iExecption) + "张表生成成功," + iExecption + "张表生成失败,共耗时" + sec + "秒！"));
            }
        }

        private void mBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BindData((Result)e.UserState);
        }

        /// <summary>
        /// 停止生成脚本
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void btnStop_Click(object sender, EventArgs e)
        {
            mBackgroundWorker.CancelAsync();
        }

        /// <summary>
        /// 对比修改的表信息
        /// </summary>
        /// <param name="list"></param>
        private bool CompareTable(List<TableInfo> list)
        {
            try
            {
                //2014-06-07修改BUG,没有需要生成的表则不进行表结构对比
                if (list == null || list.Count == 0)
                {
                    return true;
                }
                if (radioAdd.Checked)
                {
                    foreach (TableInfo t in list)
                    {
                        t.IsUpdate = false;
                    }
                    return true;
                }
                //修改，自动识别
                else if (radioUpdate.Checked || radioAuto.Checked)
                {
                    string filter = textTableList.Text;
                    if (string.IsNullOrEmpty(textTableList.Text))
                    {
                        foreach (TableInfo t in list)
                        {
                            filter = filter + t.Code + ",";
                        }
                        filter = filter.Substring(0, filter.Length - 1);
                    }
                    DataTable dt = DataDicService.GetTableInfo(filter,Properties.Settings.Default.ConnectionString);
                    foreach (TableInfo t in list)
                    {
                        if (t.ListColumnInfo == null)
                        {
                            continue;
                        }
                        DataRow[] dr = dt.Select("table_name='" + t.Code + "'");
                        if (radioAuto.Checked)
                        {
                            //新增
                            if (dr == null || dr.Count() == 0)
                            {
                                continue;
                            }
                        }
                        var query = from a in t.ListColumnInfo
                                    where !(
                                    from c in dr.AsEnumerable()
                                    select c.Field<string>("field_name")
                                    ).Contains(a.Code) 
                                    select a;
                        t.ListAddColumnInfo = query.ToList<ColumnInfo>();
                        if (radioUpdate.Checked)
                        {
                            //修改
                            if (dr == null || dr.Count() == 0)
                            {
                                t.IsUpdate = true;
                                continue;
                            }
                        }
                        string sid = dr[0]["TableGUID"].ToString();
                        if (!String.IsNullOrEmpty(sid))
                        {
                            t.TableObjectID = sid;
                        }
                        int Sequence = Convert.ToInt32(dr[0]["field_sequence"]);
                        //重新生成列序号
                        foreach (ColumnInfo info in t.ListAddColumnInfo)
                        {
                            info.Sequence = Sequence;
                            Sequence++;

                        }
                        t.IsUpdate = true;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                return false;
            }
        }

        #region "菜单栏事件"

        private void 检查更新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AutoUpdater au = new AutoUpdater();
            try
            {
                au.Update(false);
            }
            catch (WebException exp)
            {
                MessageBox.Show(String.Format("无法找到指定资源\n\n{0}", exp.Message), "自动升级", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (XmlException exp)
            {
                MessageBox.Show(String.Format("下载的升级文件有错误\n\n{0}", exp.Message), "自动升级", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (NotSupportedException exp)
            {
                MessageBox.Show(String.Format("升级地址配置错误\n\n{0}", exp.Message), "自动升级", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException exp)
            {
                MessageBox.Show(String.Format("下载的升级文件有错误\n\n{0}", exp.Message), "自动升级", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception exp)
            {
                MessageBox.Show(String.Format("升级过程中发生错误\n\n{0}", exp.Message), "自动升级", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 关于ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }


        private void 说明ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InfoForm infoForm = new InfoForm();
            infoForm.ShowDialog();
        }


        private void 数据库配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ServerLogin login = new ServerLogin();
            login.ShowDialog();
        }


        private void 特别说明ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpecialInfoForm specialInfoForm = new SpecialInfoForm();
            specialInfoForm.ShowDialog();
        }

        private void 文档生成工具ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //文档生成工具程序集名称
            string docExe = "MyTools.DataDic2Doc";
            System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo();
            //设置外部程序名         
            Info.FileName = docExe+".exe";       
            //设置外部程序工作目录为           
            Info.WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory;        
            //最小化方式启动        
            Info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            //声明一个程序类          
            System.Diagnostics.Process Proc;
            try
            {
                System.Diagnostics.Process[] p=System.Diagnostics.Process.GetProcessesByName(docExe);
                if (p != null && p.Length > 0)
                {
                    MessageBox.Show("数据字典文档生成工具已经运行！" ,"提示");
                    return;
                }
                Proc = System.Diagnostics.Process.Start(Info);
                System.Threading.Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据字典文档生成工具启动失败,"+ex.Message, "错误", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }
        }

        #endregion

       
    }

    #region "相关枚举"

    /// <summary>
    /// 执行结果
    /// </summary>
    public class Result
    {
        public Result()
        {
        }

        public Result(Level RLevel, string Message)
        {
            this.RLevel = RLevel;
            this.Message = Message;
        }

        public Result(Level RLevel)
        {
            this.RLevel = RLevel;
        }

        /// <summary>
        /// 级别
        /// </summary>
        public Level RLevel
        {
            get;
            set;
        }

        /// <summary>
        /// 类别
        /// </summary>
        public string Type
        {
            get
            {
                return GetCLevel(RLevel);
            }
        }

        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// 获取级别的中文
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static string GetCLevel(Level level)
        {
            switch (level)
            {
                case Level.Execption:
                    return "异常";
                case Level.Success:
                    return "成功";
                case Level.System:
                    return "系统";
                default:
                    return "系统";
            }
        }
    }

    /// <summary>
    /// 级别
    /// </summary>
    public enum Level
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success,
        /// <summary>
        /// 异常
        /// </summary>
        Execption,
        /// <summary>
        /// 系统提示
        /// </summary>
        System
    }

    #endregion
}
