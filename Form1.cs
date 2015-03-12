using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Data;
using System.Threading;

namespace MumiMusic
{
    public partial class Form1 : Form
    {
        private Thread thread1;
        set_Text setLrcText;
        set_Text setLableLrc;
        delegate void set_Text(string s);
        ShowLrc lrc = new ShowLrc();
        Lrc L = new Lrc();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        protected static extern bool AnimateWindow(IntPtr hWnd, int dwTime, int dwFlags);
        public const Int32 AW_BLEND = 0x00080000; 
        public const Int32 AW_CENTER = 0x00000010;
        public const Int32 AW_ACTIVATE = 0x00020000; 
        public Form1()
        {
            InitializeComponent();
            SetClassLong(this.Handle, GCL_STYLE, GetClassLong(this.Handle, GCL_STYLE) | CS_DropSHADOW); //API函数加载，实现窗体边框阴影效果
        }
        #region 窗体边框阴影效果变量申明

        const int CS_DropSHADOW = 0x20000;
        const int GCL_STYLE = (-26);
        //声明Win32 API
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetClassLong(IntPtr hwnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassLong(IntPtr hwnd, int nIndex);

        #endregion
        #region 窗体拖动代码
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }
        #endregion
        bool isplay=false;
        private void Form1_Load(object sender, EventArgs e)
        {
            AnimateWindow(this.Handle, 500, AW_BLEND | AW_CENTER | AW_ACTIVATE); 
            setLrcText = new set_Text(set_lableText);
            setLableLrc = new set_Text(set_lableLrc);
            thread1 = new Thread(new ThreadStart(SerchLrc));
            GetMusicList();
            getNum();
            try
            {
               
                this.axWindowsMediaPlayer1.settings.volume = 35;//初始化声音为35
                play(names[musicNum]);
                isplay = true;
            }
            catch (Exception)
            {

                isplay = false;
            }
                
        }
        //设置一个变量来判断是否快进
        bool next=false;
        #region 当歌词超出panle长度时 设置左右移动变换
        //int lefti=0;//设置移动次数
        int y=0;
        int bb;
        int rx;
        int lx;
        int bl;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (lx >= -100)
            {
                musicName.Location = new Point(bl - 1, y);
                bl--;
                lx--;
            }
            else
            {
                
                timer1.Enabled = false;
                timer2.Enabled = true;
            }
        }
        //获得移动次数
        public void getNum()
        {
            bl = 0;
            rx = 0;
            lx = 0;
            musicName.Location = new Point(57, 13);
            if (musicName.Size.Width > 150)
            {
                bb=bl =rx= lx = musicName.Location.X + 100;
                y = musicName.Location.Y;
                timer1.Enabled = true;
            }
            else
            {
                timer1.Enabled = false;
            }
        }
        #endregion

        private void Close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #region 按钮颜色变化
        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.preview_down;
        }
        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            if (plays == false)
            {
                pictureBox2.Image = Properties.Resources.play_down;
            }
            else
            {
                pictureBox2.Image = Properties.Resources.pause_down;
            }
            
        }
        private void pictureBox3_MouseEnter(object sender, EventArgs e)
        {
            pictureBox3.Image = Properties.Resources.next_down;
        }
        private void pictureBox4_MouseEnter(object sender, EventArgs e)
        {
            pictureBox4.Image = Properties.Resources.favorite_down;
        }
        private void pictureBox5_MouseEnter(object sender, EventArgs e)
        {
            pictureBox5.Image = Properties.Resources.list_down;
        }

        private void pictureBox5_MouseLeave(object sender, EventArgs e)
        {
            pictureBox5.Image = Properties.Resources.list_on;
        }
        private void pictureBox4_MouseLeave(object sender, EventArgs e)
        {
            pictureBox4.Image = Properties.Resources.favorite_on;
        }
        private void pictureBox3_MouseLeave(object sender, EventArgs e)
        {
            pictureBox3.Image = Properties.Resources.next_on;
        }
        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            if (plays == false)
            {
                pictureBox2.Image = Properties.Resources.play_on;
            }
            else
            {
                pictureBox2.Image = Properties.Resources.pause_on;
            }
            
        }
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.preview_on;
        }
        #endregion

        
        #region lable颜色变化
        private void TxtSkin_MouseMove(object sender, MouseEventArgs e)
        {
            TxtSkin.ForeColor = Color.Black;
        }

        private void small_MouseMove(object sender, MouseEventArgs e)
        {
            small.ForeColor = Color.Black;
        }

        private void Close_MouseMove(object sender, MouseEventArgs e)
        {
            Closes.ForeColor = Color.Black;
        }

        private void TxtSkin_MouseLeave(object sender, EventArgs e)
        {
            TxtSkin.ForeColor = Color.White;
        }

        private void small_MouseLeave(object sender, EventArgs e)
        {
            small.ForeColor = Color.White;
        }

        private void Close_MouseLeave(object sender, EventArgs e)
        {
            Closes.ForeColor = Color.White;

        }
        #endregion

        #region 音量和进度

        int panleX;//获取当前panle的X

        //音量的
        private void panel2_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        private void panel2_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Default;
        }

        private void panel1_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Default;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            voice = e.Location.X;
            setVoice(voice);
            panel2.Size = new Size(e.Location.X, 3);
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            voice = e.Location.X;
            panel2.Size = new Size(e.Location.X, 3);
            setVoice(voice);
        }

        private void pmusicdown_MouseDown(object sender, MouseEventArgs e)
        {
            pmusicup.Size = new Size(e.Location.X, 3);
            panleX = e.Location.X;
            changeTime(310, panleX);
            next = true;
            
        }

        private void pmusicdown_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        private void pmusicdown_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Default;
        }

        private void pmusicup_MouseDown(object sender, MouseEventArgs e)
        {
            pmusicup.Size = new Size(e.Location.X, 3);
            panleX = e.Location.X;
            changeTime(310, panleX);
            next = true;
        }

        private void pmusicup_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        private void pmusicup_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Default;
        }

        #endregion
        #region 皮肤panle
        Boolean skins = false;
        private void TxtSkin_Click(object sender, EventArgs e)
        {
            if (skins == false)
            {
                pskin.Visible = true;
                skins = true;
            }
            else
            {
                pskin.Visible = false;
                skins = false;
            }
        }
        string picfile;//保存copy源
        string picName;
        private void pictureBox9_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(".\\Background") == false)
            {
                Directory.CreateDirectory(".\\Background");
                OpenFileDialog of1 = new OpenFileDialog();
                of1.InitialDirectory = "c:\\";
                of1.Filter = "png|*.png|jpg|*.jpg|bmp|*.bmp";
                of1.RestoreDirectory = true;
                of1.FilterIndex = 1;
                if (of1.ShowDialog() == DialogResult.OK)
                {
                    picfile = of1.FileName;
                    picName = of1.SafeFileName;
                    try
                    {
                        File.Copy(picfile, string.Format("Background\\{0}", picName, true));
                    }
                    catch (Exception)
                    {

                        //
                    }
                    this.BackgroundImage = Image.FromFile(string.Format("Background\\{0}", picName));
                }
            }
            else
            {
                OpenFileDialog of = new OpenFileDialog();
                of.InitialDirectory = "c:\\";
                of.Filter = "png|*.png|jpg|*.jpg|bmp|*.bmp";
                of.RestoreDirectory = true;
                of.FilterIndex = 1;
                if (of.ShowDialog() == DialogResult.OK)
                {
                    picfile = of.FileName;
                    picName = of.SafeFileName;
                    try
                    {
                        File.Copy(picfile, string.Format("Background\\{0}", picName, true));
                    }
                    catch (Exception)
                    {

                        //
                    }
                    this.BackgroundImage = Image.FromFile(string.Format("Background\\{0}", picName));
                }
            }
            
        }
        private void pictureBox6_Click(object sender, EventArgs e)
        {
            this.BackgroundImage = Properties.Resources._0;
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            this.BackgroundImage = Properties.Resources._1;
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            this.BackgroundImage = Properties.Resources._2;
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            this.BackgroundImage = Properties.Resources._3;
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            this.BackgroundImage = Properties.Resources._4;
        }
        #endregion


        string[] names;//获取歌曲路径集合
        List<string> list;
        int voice;//声音
        int musicNum=0;




        //播放
        public void play(string namepath)
        {
            timer4.Enabled = false;
            if (thread1.IsAlive) 
            {
                thread1.Abort(); //撤消thread1
            }
            thread1 = new Thread(new ThreadStart(SerchLrc));
            this.axWindowsMediaPlayer1.URL = namepath;
            musicName.Text = this.axWindowsMediaPlayer1.currentMedia.name;
            thread1.Start();
            getNum();
            isplay = true;
            plays = true;
            timer3.Enabled = true;
            getmusicTime();
            if (plays == true)
            {
                pictureBox2.Image = Properties.Resources.pause_on;
            }
            
            
        }

        public void getResult()
        {
            MessageBox.Show(this.axWindowsMediaPlayer1.playState.ToString()); 
        }
        string[] lists=new string[100];
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            list=new List<string>();
            string[] oldFile;//保存以前的names
            string[] newFile;//排序后的names
            OpenFileDialog of = new OpenFileDialog();
            of.InitialDirectory = "c:\\";
            of.Filter = "mp3|*.mp3|wav|*.wav";
            of.RestoreDirectory = true;
            of.FilterIndex = 1;
            of.Multiselect = true;
            if (of.ShowDialog() == DialogResult.OK)
            {
                int k = 0;
                int same=0;//记录相同数量
                if (names == null)
                {
                    oldFile = new string[of.FileNames.Length];
                    foreach (var i in of.FileNames)
                    {
                            oldFile[k] = i;
                            k++;
                    }
                }
                else
                {
                    oldFile = new string[of.FileNames.Length + names.Length];
                    for (int y = 0; y < names.Length; y++)
                    {
                        oldFile[k] = names[y];
                        k++;
                    }
                    foreach (var i in of.FileNames)
                    {
                        oldFile[k] = i;
                        k++;

                    }
                }
                for (int i = 0; i < oldFile.Length; i++)
                {
                    for (int j = i + 1; j < oldFile.Length; j++)
                    {
                        if (oldFile[i] == oldFile[j])
                        {
                            same++;
                        }
                    }
                }
                for (int i = 0; i < oldFile.Length; i++)
                {
                    for (int j = i + 1; j < oldFile.Length; j++)
                    {
                        if (oldFile[i] == oldFile[j])
                        {
                            oldFile[i] = "null";
                        }
                    }
                }
                //消除重复歌曲
                int w=0;
                newFile = new string[oldFile.Length - same];
                for (int i = 0; i < oldFile.Length; i++)
                {
                    if (oldFile[i] != "null")
                    {
                        newFile[w] = oldFile[i];
                        w++;
                    }
                }
                
                names = newFile;
                for (int i = 0; i < names.Length; i++)
                {
                    list.Add(names[i]);
                }
                    SaveMusicList();
            }
        }

        //save方法
        public void SaveMusicList()
        {
            if (File.Exists(".\\Music.lst") == true)
            {
                File.Delete(".\\Music.lst");
            }
            SaveFileDialog sf = new SaveFileDialog();
            sf.FileName = "Music.lst";
            sf.RestoreDirectory = true;
            sf.FilterIndex = 1;
            FileStream fs = new FileStream(string.Format("{0}", sf.FileName), FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, list);
            fs.Close();
            musicNum = names.Length-1;
            play(names[musicNum]);
        }
        
        //读取方法
        public void GetMusicList()
        {
            string[] musicFile;
            if (File.Exists(".\\Music.lst") == false)//如果不存在就创建file文件夹
            {
            }
            else
            {
                OpenFileDialog of = new OpenFileDialog();
                of.FileName = "Music.lst";
                of.RestoreDirectory = true;
                of.FilterIndex = 1;
                FileStream fs = new FileStream(string.Format("{0}", of.FileName), FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                this.list = ((List<string>)bf.Deserialize(fs));
                fs.Close();
                musicFile = new string[list.Count];
                for (int i=0;i<list.Count;i++)
                {
                    musicFile[i] = list[i];
                }
                names = musicFile;
            }
        }

        Boolean plays=false;
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            
            if (plays == false)
            {
                this.axWindowsMediaPlayer1.Ctlcontrols.play();
                timer3.Enabled = true;
                pictureBox2.Image = Properties.Resources.pause_down;
                plays = true;
            }
            else
            {
                pictureBox2.Image = Properties.Resources.play_down;
                this.axWindowsMediaPlayer1.Ctlcontrols.pause();
                timer3.Enabled = false;
                plays = false;
            }
        }

        //设置声音大小

        public void setVoice(int voice)
        {
            this.axWindowsMediaPlayer1.settings.volume = voice;
        }

        //设置透明度的方法
        int op;//透明度
        public void setOpacity(int op)
        {
            this.Opacity = (double)op / 100;
        }
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            //
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (isplay == true)
            {
                getmusicTime();
                this.label1.Text = this.axWindowsMediaPlayer1.Ctlcontrols.currentPositionString;
                this.label2.Text = this.axWindowsMediaPlayer1.currentMedia.durationString;
                if (this.axWindowsMediaPlayer1.playState.ToString() == "wmppsStopped")
                {
                    timer1.Enabled = false;
                    label1.Text = "00:00";
                    try
                    {
                        musicNum++;
                        play(names[musicNum]);
                    }
                    catch (Exception)
                    {

                        timer3.Enabled = false;
                    }
                }
            }
            else
            {
                
            }

        }

        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            panel4.Size = new Size(e.Location.X, 3);
            op = e.Location.X;
            setOpacity(op);
        }

        private void panel3_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        private void panel3_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Default;
        }

        private void panel4_MouseDown(object sender, MouseEventArgs e)
        {
            panel4.Size = new Size(e.Location.X, 3);
            op = e.Location.X;
            setOpacity(op);
        }

        private void panel4_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        private void panel4_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Default;
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            musicNum--;
            try
            {
                play(names[musicNum]);
            }
            catch (Exception)
            {

                musicNum += 1;
            }
        }


        double alltime;//全部时间
        double thistime;//当前时间
        double bfb;//百分比
        double thisX;
        //改变进度条长度
        public void getmusicTime()
        {
            thistime = this.axWindowsMediaPlayer1.Ctlcontrols.currentPosition;
            alltime = this.axWindowsMediaPlayer1.currentMedia.duration;
            bfb = thistime / alltime;
            thisX = 310*bfb;
            pmusicup.Size = new Size((int)thisX,3);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            musicNum++;
            try
            {
                play(names[musicNum]);
            }
            catch (Exception)
            {

                musicNum -= 1;
            }
        }

        private void small_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            musicName.Location = new Point(300, y);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (lx < 57)
            {
                musicName.Location = new Point(bl + 1, y);
                bl++;
                lx++;
            }
            else
            {
                timer2.Enabled = false;
                timer1.Enabled = true;
            }
        }

        //获取当前进度
        double Alltime;
        double thisTime;
        Double b;
        public void changeTime(double all,double thisp)
        {
            try
            {
                b = thisp / all;
                Alltime = this.axWindowsMediaPlayer1.currentMedia.duration;
                thisTime = Alltime * b;
                this.axWindowsMediaPlayer1.Ctlcontrols.currentPosition = thisTime;
            }
            catch (Exception)
            {
                
               //
            }
        }

        private void pmusicup_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.SetToolTip(pmusicup, label1.Text);
        }

        private void pmusicdown_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.SetToolTip(pmusicdown, label1.Text);
        }
        string title;

        private void SerchLrc()
        {
            label4.Invoke(setLrcText , new object[] { "正在搜索歌词..." });
            title = axWindowsMediaPlayer1.currentMedia.getItemInfo("Title");
            string[] sArray = title.Split('-');
            title = sArray[sArray.Length - 1];
            ChangeLable(L.getLrc(title.Trim()));
        }

        string[] Ltime=new string[200];//时间
        string[] Ltext=new string[200];//歌词
        bool timer=false;
        /// <summary>
        /// 改变歌词lable的方法 并且加载显示歌词方法
        /// </summary> 
        /// <param name="text">传入的返回值</param>
        private void ChangeLable(string text)
        {
            Ltime = new string[200];
            Ltext = new string[200];
            if (text == "歌词找到并下载成功！" || text == "正在解析歌词...")
            {
                label4.Invoke(setLrcText, new object[] { text });
                lrc.getLrc(string.Format(".\\Lrc\\{0}.Lrc", L.returnPath()));
                Ltext = lrc.returnText();
                Ltime = lrc.returnTime();
                label4.Invoke(setLableLrc, new object[] { Ltext[0] });
                timer = true;
            }
            else
            {
                label4.Invoke(setLableLrc, new object[] { text });
            }
        }
        /// <summary>
        /// 多线程给lable传值
        /// </summary>
        /// <param name="s"></param>
        private void set_lableText(string s)
        {
            label4.Text = s;
        }
        private void set_lableLrc(string text)
        {
            label4.Text = text;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (thread1.IsAlive) //判断thread1是否存在，不能撤消一个不存在的线程，否则会引发异常
            {
                thread1.Abort(); //撤消thread1
            }
        }

        

        /// <summary>
        /// 显示歌词方法
        /// </summary>
        public void showLrc()
        {
            timer4.Enabled = true;
        }

        //计算左右偏移
        string BigTime;
        string SmallTime;
        public void LeftRight()
        {
            string time = this.axWindowsMediaPlayer1.Ctlcontrols.currentPositionString + ":00";
            try
            {
                int start = int.Parse(time.Substring(0, 2));
                string zj = time.Substring(3, 2);
                int zjnum = int.Parse(zj);
                BigTime = time.Substring(0, 3) + (zjnum + 2).ToString() + ":00";
                if (zjnum >= 2)
                {
                    SmallTime = time.Substring(0, 3) + (zjnum - 2).ToString() + ":00";
                }
                else if (zjnum == 00 && start > 0)
                {
                    SmallTime = "0" + (start - 1).ToString() + ":" + 58.ToString() + ":00";
                }
            }
            catch (Exception)
            {
                
            }
            
            
        }
        private void timer4_Tick(object sender, EventArgs e)
        {
            string time;
            time = this.axWindowsMediaPlayer1.Ctlcontrols.currentPositionString +":00";
            if (next == true)
            {
                LeftRight();
                for (int i = 0; i < 100; i++)
                {
                    if ( Ltime[i]==BigTime||Ltime[i]==SmallTime)
                    {
                        label4.Text = Ltext[i];
                        next = false;
                    }
                }
            }
            for (int i = 0; i < 100; i++)
            {
                if (time == Ltime[i])
                {
                    label4.Text = Ltext[i];
                }
            }
        }
        /// <summary>
        /// timer5用来监听timer4 因为在线程里无法操作timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer5_Tick(object sender, EventArgs e)
        {
            if (timer == true)
            {
                timer4.Enabled = true;
            }
            else if (timer == false)
            {
                timer4.Enabled = false;
            }
        }

        //绘制界面
        private void Draw(Graphics formGp)
        {
            Bitmap bitmap = new Bitmap(300, 100);
            Graphics gp = Graphics.FromImage(bitmap);
            //this.DrawBg(gp);
            //if (this.list != null)
            //{
            //    this.list.Draw(gp);
            //}
            //this.DrawCountClick(gp);

            formGp.DrawImage(bitmap, 200, 200);
        }

        private void timer6_Tick(object sender, EventArgs e)
        {
            
        }
    }
}
