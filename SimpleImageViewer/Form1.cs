using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleImageViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        int ed, ex, ey;

        int monitor = 1;

        string path = "";

        string[] files;

        bool AttributesRead = false;


        int baseWidth, baseHeight;
        Image baseImage;

        Image loadImage;

        double scale = 0;

        Rectangle resolution;

        Rectangle oldDisplayedRectangle;

        int dX = 0, dY = 0;

        List<string> images = new List<string>();
        int index = -1;

        string supportedExtensions = "*.jpg,*.png,*.bmp,*.jpe,*.jpeg";

        bool subDirectoriesScanned = false;



        private void Form1_Load(object sender, EventArgs e)
        {
            listView1.Columns.Add("", 100);

            listView1.Columns.Add("", 160);


            /*
            ListViewItem item = new ListViewItem("viens");
            item.SubItems.Add("OBANA123");

            ListViewItem item1 = new ListViewItem("du");
            item1.SubItems.Add("OBANA123");

            ListViewItem item2 = new ListViewItem("tryyys");
            item2.SubItems.Add("OBANA123");


            listView1.Items.Add(item);
            listView1.Items.Add(item1);
            listView1.Items.Add(item2);


            listView1.BackColor = Color.FromArgb(33, 33, 33);
            listView1.ForeColor = Color.Lime;

            listView1.Items[0].BackColor = Color.FromArgb(33, 33, 33);
            listView1.Invalidate();
            */


            pictureBox1.Controls.Add(label2);
            pictureBox1.Controls.Add(pictureBox2);
            pictureBox2.Image = Properties.Resources.left;

            monitor = SimpleImageViewer.Properties.Settings.Default.Monitor;

            resolution = Screen.AllScreens[monitor].Bounds;

            label1.Text = "";
            label1.Visible = false;
            //label2.Text = "";


            listView1.Left = (resolution.Width - listView1.Width) / 2;
            listView1.Top = (resolution.Height - listView1.Height);


            //BackColor = Color.FromArgb(33, 33, 33);
            this.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);




            string[] args = Environment.GetCommandLineArgs();

            //Different pictures for testing

            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\DSC_3314.jpg" }; //portrait
            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\DSC_3432.jpg" }; //landscape
            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\168937473_4522624104419534_2925807893586856406_n.jpg" }; // meme 276x370
            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\2.9.22.12.11.jpg" }; //wide ss
            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\verybig.jpg" }; //very big
            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\perfect.png" }; //resolution size
            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\IMG_7650.jpg" }; //has a lot of attributes
            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\Captures\\2.4.13.40.42.jpg" }; //folder

            //args = new string[] { "", "E:\\PHOTO ARCHIVE\\2.9.22.12.11.jpg" }; //toli

            //args = new string[] { "", "G:\\ivadinis.png" }; //ivadinis

            {
                /*
                var imgKey = Registry.ClassesRoot.OpenSubKey(".bmp");
                var imgType = imgKey.GetValue("");
                String myExecutable = Assembly.GetEntryAssembly().Location;
                String command = "\"" + myExecutable + "\"" + " \"%1\"";
                String keyName = imgType + @"\shell\Open\command";
                using (var key = Registry.ClassesRoot.CreateSubKey(keyName))
                {
                    key.SetValue("", command);
                }
                */
            }///registry


            if (args.Length > 1)
            {
                path = args[1];

                Image img = Image.FromFile(args[1]);

                
                string root = Path.GetDirectoryName(path);

                this.Size = new Size(resolution.Width, resolution.Height);
                pictureBox1.BackColor = Color.FromArgb(33, 33, 33);
                pictureBox1.Size = new Size(resolution.Width, resolution.Height);
                this.Location = new Point(resolution.X, resolution.Y);

                if (img.PropertyIdList.Contains(0x0112))
                {
                    int orientation = img.GetPropertyItem(0x0112).Value[0];

                    if (orientation == 6)
                    {
                        img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    }
                    else if (orientation == 8)
                    {
                        img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    }
                    else if (orientation == 3)
                    {
                        img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    }
                    else if (orientation != 1)
                    {
                        //MessageBox.Show("Unknown orientation: " + orientation);
                    }

                }

                if (img.Width < resolution.Width && img.Height < resolution.Height)
                {
                    pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                }
                else
                {
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }


                listView1.BackColor = Color.FromArgb(33, 33, 33);

                baseImage = img;

                baseWidth = baseImage.Width;
                baseHeight = baseImage.Height;

                oldDisplayedRectangle = new Rectangle(0, 0, img.Width, img.Height);
                //pictureBox1.Image = baseImage;

                bool widePicture = (double)baseWidth / resolution.Width > (double)baseHeight / resolution.Height;
                int displayWidth, displayHeight;
                if (widePicture)
                {
                    displayWidth = resolution.Width;
                    displayHeight = img.Height * resolution.Width / img.Width;
                }
                else
                {
                    displayHeight = resolution.Height;
                    displayWidth = img.Width * resolution.Height / img.Height;
                }

                //displayHeight /= sizeReduceFactor;
                //displayWidth /= sizeReduceFactor;

                Bitmap bmp = new Bitmap(displayWidth, displayHeight);

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(baseImage, new Rectangle(0, 0, displayWidth, displayHeight), oldDisplayedRectangle, GraphicsUnit.Pixel);
                }

                pictureBox1.Image = bmp;
                pictureBox1.Invalidate();
                //pictureBox1.Image = Manipulator.ZoomImage(-1, resolution.Width / 2, resolution.Height / 2, baseImage, ref oldDisplayedRectangle, 1, pictureBox1, resolution);
                images.Add(path);
                index++;
                
                files = Directory.GetFiles(root, "*.*").Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            }
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0 && scale == 0)
            {
                return;
            }

            label1.Visible = false;

            bool smallImage = baseWidth < resolution.Width && baseHeight < resolution.Height;


            if (smallImage)
            {
                if (e.Delta > 0)
                {
                    if (pictureBox1.SizeMode == PictureBoxSizeMode.CenterImage)
                    {
                        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    else
                    {
                        scale += e.Delta / Math.Abs(e.Delta);
                    }
                }
                else
                {
                    if (scale == 0)
                    {
                        pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                    }
                    else
                    {
                        scale += e.Delta / Math.Abs(e.Delta);
                    }
                }
            }
            else
            {
                if (scale > 0 || e.Delta > 0)
                {
                    scale += e.Delta / Math.Abs(e.Delta);
                }
                else
                {
                    //return;
                }
            }



            //scale += e.Delta / Math.Abs(e.Delta);

            DateTime dt0 = DateTime.Now;
            //Image img;

            ed = e.Delta;
            ex = e.X;
            ey = e.Y;



            if (e.Delta > 0)
            {

                /*
            if (backgroundWorker1.IsBusy)
            {

                backgroundWorker1.CancelAsync();

                while (!backgroundWorker1.IsBusy)
                    backgroundWorker1.RunWorkerAsync();

            }
            else
            {
                backgroundWorker1.RunWorkerAsync();
            }
                */


                /*
                Rectangle rect = Manipulator.GetZoomedRectangle(e.Delta, e.X, e.Y, baseWidth, baseHeight, oldDisplayedRectangle, scale, resolution);
                oldDisplayedRectangle = rect;
                img = Manipulator.ZoomInImage(e.X, e.Y, pictureBox1.Image, resolution, 10);
                */

            }
            else
            {
                //Console.WriteLine("without");

                //double bound = scale;

                //for (int i = 0; i < 10; i++)
                //{
                //scale = bound + 1 - ((double)i / 9);

                /*
                if (!backgroundWorker1.IsBusy)
                {
                    //Console.WriteLine("Start main");
                    backgroundWorker1.RunWorkerAsync(10);
                }
                else
                {
                    //Console.WriteLine("Start Second");
                    BackgroundWorker bw = new BackgroundWorker();
                    bw.DoWork += backgroundWorker1_DoWork;
                    bw.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
                    bw.RunWorkerAsync(10);
                }

                */

                //}

                //img = Manipulator.ZoomImage(e.Delta, e.X, e.Y, baseImage, ref oldDisplayedRectangle, scale, resolution, 10);
            }

            //pictureBox1.Image.Dispose();

            //pictureBox1.Image = loadImage;


            //Console.WriteLine("Start Second");
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += backgroundWorker1_DoWork;
            bw.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            bw.RunWorkerAsync(new Tuple<int, Image>(1, (Bitmap)baseImage.Clone()));



            DateTime dt1 = DateTime.Now;
            label1.Text = '\n'.ToString() + (dt1 - dt0).TotalMilliseconds.ToString();

            //label1.Visible = true;
            if (label1.Text.Length > 350)
            {
                label1.Text = "";
            }
        }

        private void SetFullImage(Image img)
        {
            AttributesRead = false;

            scale = 0;

            pictureBox1.Image.Dispose();
            if (img.PropertyIdList.Contains(0x0112))
            {
                int orientation = img.GetPropertyItem(0x0112).Value[0];

                if (orientation == 6)
                {
                    img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                }
                else if (orientation == 8)
                {
                    img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                }
                else if (orientation == 3)
                {
                    img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                }
                else if (orientation != 1)
                {
                    //MessageBox.Show("Unknown orientation: " + orientation);
                }

            }

            if (img.Width < resolution.Width && img.Height < resolution.Height)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            else
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }

            oldDisplayedRectangle = new Rectangle(0, 0, resolution.Width, resolution.Height);

            label2.Text = Path.GetFileName(path);
            baseWidth = img.Width;
            baseHeight = img.Height;
            pictureBox1.Image = img;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            label1.Visible = false;

            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
            else if (e.KeyCode == Keys.Left)
            {
                scale = 0;
                ImagePrevious();
            }
            else if (e.KeyCode == Keys.Right)
            {
                ImageNext();
            }
            else if (e.KeyCode == Keys.R)
            {
                ImageRandom();
            }
            else if (e.KeyCode == Keys.Q)
            {
                ImageRandomPrevious();
            }
            else if (e.KeyCode == Keys.C)
            {
                if (subDirectoriesScanned)
                    return;

                //files = new DirectoryInfo(Path.GetDirectoryName(path)).GetFiles("*", SearchOption.AllDirectories).Where(x => (x.Attributes & FileAttributes.Hidden) == 0).Where(s => supportedExtensions.Contains(Path.GetExtension(s.FullName).ToLower())).Select(a => a.FullName).ToArray();
                //files = Directory.GetFiles(Path.GetDirectoryName(path), "*", SearchOption.TopDirectoryOnly);
                files = Directory.GetFiles(Path.GetDirectoryName(path), "*", SearchOption.AllDirectories).Where(s => ( ( (new FileInfo(s)).Attributes != FileAttributes.Hidden)) && (!s.Contains("$RECYCLE")) && supportedExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
                subDirectoriesScanned = true;

                label1.Text = "Files: " + files.Length.ToString();
                label1.Visible = true;

                //ImageRandom();
            }
            else if (e.KeyCode == Keys.Z)
            {
                //ImageRandomPrevious();
            }
            else if (e.KeyCode == Keys.Back)
            {
                Close();
            }
        }

        private void ImageRandomPrevious()
        {
            if (index > 0)
            {
                //images.RemoveAt(index);
                index--;
                path = images[index];
                baseImage = Image.FromFile(path);

                SetFullImage(baseImage);
            }
        }

        private void ImageNext()
        {
            int id = Array.IndexOf(files, path);

            if (id < files.Length - 1)
            {
                if (index < images.Count - 1)
                {

                }

                baseImage = Image.FromFile(files[id + 1]);

                path = files[id + 1];
                images.Add(path);
                index++;
                SetFullImage(baseImage);
            }

        }

        private void ImagePrevious()
        {
            int id = Array.IndexOf(files, path);

            if (id > 0)
            {
                baseImage = Image.FromFile(files[id - 1]);
                path = files[id - 1];

                if (index > 0)
                {
                    images.RemoveAt(index);
                    index--;
                }
                SetFullImage(baseImage);
            }
        }

        private void ImageRandom()
        {
            int id;

            if (index < images.Count - 1)
            {

                id = index + 1;
                path = images[id];
            }
            else
            {
                Random rnd = new Random();
                id = rnd.Next(files.Length);
                path = files[id];
                images.Add(path);
            }

            baseImage = Image.FromFile(path);
            
            index++;

            SetFullImage(baseImage);
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            scale = 0;
            pictureBox1.Image = baseImage;
            oldDisplayedRectangle = new Rectangle(0, 0, baseWidth, baseHeight);

            if (baseWidth < resolution.Width && baseHeight < resolution.Height)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {


            //label1.Visible = true;
            if (e.Button == MouseButtons.Left)
            {

                //pictureBox1.Image = ZoomImage(0, e.X, e.Y, baseImage);
                //DateTime dt0 = DateTime.Now;
                oldDisplayedRectangle.X += (dX - e.X) * oldDisplayedRectangle.Width / resolution.Width;
                oldDisplayedRectangle.Y += (dY - e.Y) * oldDisplayedRectangle.Height / resolution.Height;

                //Rectangle rect = oldDisplayedRectangle;

                oldDisplayedRectangle = Manipulator.GetZoomedRectangle(0, e.X, e.Y, baseWidth, baseHeight, oldDisplayedRectangle, scale, resolution);

                ed = 0;
                ex = 0;
                ey = 0;

                /*
                bool widePicture = (double)baseWidth / resolution.Width > (double)baseHeight / resolution.Height;

                int displayWidth, displayHeight;
                if (widePicture)
                {
                    displayWidth = resolution.Width;
                    displayHeight = oldDisplayedRectangle.Height * resolution.Width / oldDisplayedRectangle.Width;
                }
                else
                {
                    displayHeight = resolution.Height;
                    displayWidth = oldDisplayedRectangle.Width * resolution.Height / oldDisplayedRectangle.Height;
                }
                */
                //Rectangle rect = new Rectangle(oldDisplayedRectangle.X + (dX - e.X) * oldDisplayedRectangle.Width / resolution.Width, oldDisplayedRectangle.Y + (dY - e.Y) * oldDisplayedRectangle.Height / resolution.Height, oldDisplayedRectangle.Width, oldDisplayedRectangle.Height);


                /*
                Bitmap bmp = new Bitmap(oldDisplayedRectangle.Width, oldDisplayedRectangle.Height);

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(baseImage, new Rectangle(0, 0, oldDisplayedRectangle.Width, oldDisplayedRectangle.Height), rect, GraphicsUnit.Pixel);
                }

                */



                /*
                if (!backgroundWorker1.IsBusy)
                {
                    Console.WriteLine("Start main");
                    backgroundWorker1.RunWorkerAsync(10);
                }
                else
                {
                    Console.WriteLine("Start Second");
                    BackgroundWorker bw = new BackgroundWorker();
                    bw.DoWork += backgroundWorker1_DoWork;
                    bw.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
                    bw.RunWorkerAsync(10);
                }
                */


                //Console.WriteLine("Start Second");
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += backgroundWorker1_DoWork;
                bw.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;

                bw.RunWorkerAsync(new Tuple<int, Image>(1, (Bitmap)baseImage));

                //DateTime dt1 = DateTime.Now;

                //Console.WriteLine('\n'.ToString() + (dt1 - dt0).TotalMilliseconds.ToString());

                /*
                label1.Text += '\n'.ToString() + (dt1 - dt0).TotalMilliseconds.ToString(); 

                if (label1.Text.Length > 350)
                {
                    label1.Text = "";
                }
                */

                //pictureBox1.Image.Dispose();
                //pictureBox1.Image = bmp;
                //pictureBox1.Invalidate();

                dX = e.X;
                dY = e.Y;
                //oldDisplayedRectangle = rect;
            }
            else
            {

                label2.Visible = false;
                pictureBox2.Visible = false;


                int X = e.X;
                int Y = e.Y;

                int reactionSize = 150;

                pictureBox2.Width = 100;
                pictureBox2.Height = 100;
                pictureBox2.Image.Dispose();


                if (X >= resolution.Width - reactionSize && Y <= reactionSize) //top right
                {
                    pictureBox2.Image = Properties.Resources.close;
                    pictureBox2.Left = resolution.Width - pictureBox2.Width;
                    pictureBox2.Top = 0;
                    pictureBox2.Visible = true;
                }
                else if (X >= resolution.Width - reactionSize && Y >= resolution.Height - reactionSize) //bottom right
                {
                }
                else if (X <= reactionSize && Y >= resolution.Height - reactionSize) // bottom left
                {
                    pictureBox2.Image = Properties.Resources.monitor;
                    pictureBox2.Left = 0;
                    pictureBox2.Top = resolution.Height - pictureBox2.Width;
                    pictureBox2.Visible = true;
                }
                else if (X <= reactionSize && Y <= reactionSize) // top left
                {
                    pictureBox2.Image = Properties.Resources.minimize;
                    pictureBox2.Left = 0;
                    pictureBox2.Top = 0;
                    pictureBox2.Visible = true;
                }
                else if (X >= resolution.Width - reactionSize && Math.Abs(Y - resolution.Height / 2) <= 2 * reactionSize) // middle right
                {
                    pictureBox2.Image = Properties.Resources.right;
                    pictureBox2.Left = resolution.Width - pictureBox2.Width;
                    pictureBox2.Top = resolution.Height / 2 - pictureBox2.Height / 2;
                    pictureBox2.Visible = true;
                }
                else if (X <= reactionSize && Math.Abs(Y - resolution.Height / 2) <= 2 * reactionSize) // middle left
                {
                    pictureBox2.Image = Properties.Resources.left;
                    pictureBox2.Left = 0;
                    pictureBox2.Top = resolution.Height / 2 - pictureBox2.Height / 2;
                    pictureBox2.Visible = true;
                }
                else if (Math.Abs(X - resolution.Width / 2) <= 2 * reactionSize && Y < reactionSize) // middle top
                {
                    label2.Text = Path.GetFullPath(path);
                    label2.Top = 5;
                    label2.Left = resolution.Width / 2 - (int)label2.CreateGraphics().MeasureString(label2.Text, label2.Font).Width / 2;
                    label2.Visible = true;
                    label2.BackColor = Color.FromArgb(120, 33, 33, 33);
                }
                else if (Math.Abs(X - resolution.Width / 2) <= 2 * reactionSize && Y >= resolution.Height - reactionSize) // Bottom middle
                {
                    listView1.Visible = true;

                    if (!AttributesRead)
                    {
                        FillListView();
                    }

                    return;
                }
                else
                {

                }

                listView1.Visible = false;
            }

            
        }

        private void FillListView()
        {
            listView1.Items.Clear();

            ListViewItem item0 = new ListViewItem("Width");
            item0.SubItems.Add(baseWidth.ToString());
            item0.BackColor = Color.FromArgb(33, 33, 33);
            item0.ForeColor = Color.Lime;

            ListViewItem item1 = new ListViewItem("Height");
            item1.SubItems.Add(baseHeight.ToString());
            item1.BackColor = Color.FromArgb(33, 33, 33);
            item1.ForeColor = Color.Lime;

            ListViewItem item2 = new ListViewItem("Date created");
            item2.SubItems.Add(File.GetCreationTime(path).ToString());
            item2.BackColor = Color.FromArgb(33, 33, 33);
            item2.ForeColor = Color.Lime;

            /*
            ListViewItem item3 = new ListViewItem("Date modified");
            item3.SubItems.Add(File.GetLastWriteTime(path).ToString());
            item3.BackColor = Color.FromArgb(33, 33, 33);
            item3.ForeColor = Color.Lime;
            */


            listView1.Items.Add(item0);
            listView1.Items.Add(item1);
            listView1.Items.Add(item2);
            //listView1.Items.Add(item3);

            AttributesRead = true;
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            
            /*
            //pictureBox2.Image.Dispose();
            //pictureBox2.BackColor = Color.Transparent;
            //pictureBox2.Image = new Bitmap(1920, 1080, PixelFormat.Format32bppArgb);
            pictureBox2.Image.Dispose();
            Bitmap bmp = new Bitmap(1920, 1080, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                
                //g.FillRectangle(new SolidBrush(Color.Transparent), 0, 0, 1920, 1080);
                g.Clear(Color.Transparent);

                Pen pen = new Pen(Color.Orange, 4);
                g.DrawRectangle(pen, e.X - 100, e.Y - 75, 200, 150);
                //pictureBox2.Invalidate();
            }
            pictureBox2.Image = bmp;
            */
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int X = e.X;
            int Y = e.Y;

            int reactionSize = 150;

            if (X >= resolution.Width - reactionSize && Y <= reactionSize) //top right
            {
                Close();
            }
            else if (X >= resolution.Width - reactionSize && Y >= resolution.Height - reactionSize) //bottom right
            {

            }
            else if (X <= reactionSize && Y >= resolution.Height - reactionSize) // bottom left
            {
                monitor = ((monitor + 1) % Screen.AllScreens.Length);
                SimpleImageViewer.Properties.Settings.Default.Monitor = monitor;
                Properties.Settings.Default.Save();
                resolution = Screen.AllScreens[monitor].Bounds;

                Image baseImage = Image.FromFile(path);
                //Task.Factory.StartNew(() => SetImage(img));
                SetFullImage(baseImage);

                this.Size = new Size(resolution.Width, resolution.Height);
                pictureBox1.Size = new Size(resolution.Width, resolution.Height);
                this.Location = new Point(resolution.X, resolution.Y);
            }
            else if (X <= reactionSize && Y <= reactionSize) // top left
            {
                WindowState = FormWindowState.Minimized;
            }
            else if (X >= resolution.Width - reactionSize && Math.Abs(Y - resolution.Height / 2) <= 2 * reactionSize) // middle right
            {
                ImageNext();
            }
            else if (X <= reactionSize && Math.Abs(Y - resolution.Height / 2) <= 2 * reactionSize) // middle left
            {
                ImagePrevious();
            }
            else if (Math.Abs(X - resolution.Width / 2) <= 2 * reactionSize && Y <= reactionSize) // middle top
            {

            }
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            int X = e.X + pictureBox2.Location.X;
            int Y = e.Y + pictureBox2.Location.Y;

            int reactionSize = 150;

            if (X >= resolution.Width - reactionSize && Y <= reactionSize) //top right
            {
                Close();
            }
            else if (X >= resolution.Width - reactionSize && Y >= resolution.Height - reactionSize) //bottom right
            {

            }
            else if (X <= reactionSize && Y >= resolution.Height - reactionSize) // bottom left
            {
                monitor = ((monitor + 1) % Screen.AllScreens.Length);
                SimpleImageViewer.Properties.Settings.Default.Monitor = monitor;
                Properties.Settings.Default.Save();
                resolution = Screen.AllScreens[monitor].Bounds;

                Image baseImage = Image.FromFile(path);
                //Task.Factory.StartNew(() => SetImage(img));
                SetFullImage(baseImage);

                this.Size = new Size(resolution.Width, resolution.Height);
                pictureBox1.Size = new Size(resolution.Width, resolution.Height);
                this.Location = new Point(resolution.X, resolution.Y);
            }
            else if (X <= reactionSize && Y <= reactionSize) // top left
            {
                WindowState = FormWindowState.Minimized;
            }
            else if (X >= resolution.Width - reactionSize && Math.Abs(Y - resolution.Height / 2) <= 2 * reactionSize) // middle right
            {
                ImageNext();
            }
            else if (X <= reactionSize && Math.Abs(Y - resolution.Height / 2) <= 2 * reactionSize) // middle left
            {
                ImagePrevious();
            }
            else if (Math.Abs(X - resolution.Width / 2) <= 2 * reactionSize && Y <= reactionSize) // middle top
            {
                //MessageBox.Show("");
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Default;
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(33, 33, 33)), e.Bounds);
            
            //e.DrawBackground();
            e.DrawText();
            e.Graphics.DrawString(Path.GetFileName(path), DefaultFont, Brushes.Lime, 0, 0);

            /*
            using (var sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;

                using (var headerFont = new Font("Microsoft Sans Serif", 9, FontStyle.Bold))
                {
                    e.Graphics.DrawString(e.Header.Text, headerFont,
                        Brushes.Lime, e.Bounds, sf);
                }
            }
            */
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void label2_DoubleClick(object sender, EventArgs e)
        {
            // this.WindowState = FormWindowState.Minimized;
            Process.Start("explorer.exe", @"/select," + path);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Tuple<int,Image> args = (Tuple<int, Image>)(e.Argument);

            int level = args.Item1;

            Image img = args.Item2;

            //Console.WriteLine("Start: " + level);
            //Console.WriteLine("Start");
            //Console.WriteLine("normal");
            DateTime dt0 = DateTime.Now;


            //GCHandle gch = GCHandle.Alloc(img, GCHandleType.Normal);
            //IntPtr pObj = gch.();

            //Console.WriteLine(pObj.ToString());


            oldDisplayedRectangle = Manipulator.GetRectangle(oldDisplayedRectangle, resolution, ed, ex, ey, baseImage.Size, scale);


            DateTime dt1 = DateTime.Now;

            loadImage = Manipulator.ZoomImage(img, oldDisplayedRectangle, resolution, level);

            //Console.WriteLine("End: " + level);

            DateTime dt2= DateTime.Now;

            //Console.WriteLine(level + " Rect: " + (dt1 - dt0).TotalMilliseconds.ToString());
            //Console.WriteLine(level + " Draw: " + (dt2 - dt1).TotalMilliseconds.ToString());
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Console.WriteLine(e.Error.Message);
            }
            pictureBox1.Image = loadImage;
            //Console.WriteLine("End");
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.SizeAll;
            dX = e.X;
            dY = e.Y;
        }
    }
}
