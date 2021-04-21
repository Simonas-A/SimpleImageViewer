using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
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


        int baseWidth, baseHeight;
        Image baseImage;

        Image loadImage;

        int scale = 0;

        Rectangle resolution;

        Rectangle oldDisplayedRectangle;

        int dX = 0, dY = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.Controls.Add(label2);
            pictureBox1.Controls.Add(pictureBox2);
            pictureBox2.Image = Properties.Resources.left;

            monitor = SimpleImageViewer.Properties.Settings.Default.Monitor;

            resolution = Screen.AllScreens[monitor].Bounds;

            label1.Text = "";
            label1.Visible = false;
            //label2.Text = "";


            //BackColor = Color.FromArgb(33, 33, 33);
            this.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);




            string[] args = Environment.GetCommandLineArgs();

            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\DSC_3314.jpg" }; //portrait
            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\DSC_3432.jpg" }; //landscape
            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\168937473_4522624104419534_2925807893586856406_n.jpg" }; // meme 276x370
            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\2.9.22.12.11.jpg" }; //wide ss

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

                string supportedExtensions = "*.jpg,*.png,*.bmp,*.jpe,*.jpeg";
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

                
                files = Directory.GetFiles(root, "*.*").Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            }
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
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
            Image img;

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

                Rectangle rect = Manipulator.GetZoomedRectangle(e.Delta, e.X, e.Y, baseWidth, baseHeight, oldDisplayedRectangle, scale, resolution);
                oldDisplayedRectangle = rect;
                img = Manipulator.ZoomInImage(e.X, e.Y, pictureBox1.Image, resolution, 2);

            }
            else
            {
                img = Manipulator.ZoomImage(e.Delta, e.X, e.Y, baseImage, ref oldDisplayedRectangle, scale, resolution, 5);
            }

            //pictureBox1.Image.Dispose();
            
            pictureBox1.Image = img;

            if (!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();

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
                
            }
        }

        private void ImageNext()
        {
            int id = Array.IndexOf(files, path);

            if (id < files.Length - 1)
            {
                baseImage = Image.FromFile(files[id + 1]);

                path = files[id + 1];
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
                SetFullImage(baseImage);
            }
        }

        private void ImageRandom()
        {
            Random rnd = new Random();
            int id = rnd.Next(files.Length);
            path = files[id];
            baseImage = Image.FromFile(files[id]);
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
                DateTime dt0 = DateTime.Now;
                oldDisplayedRectangle.X += (dX - e.X) * oldDisplayedRectangle.Width / resolution.Width;
                oldDisplayedRectangle.Y += (dY - e.Y) * oldDisplayedRectangle.Height / resolution.Height;

                Rectangle rect = oldDisplayedRectangle;

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
                Bitmap bmp = new Bitmap(oldDisplayedRectangle.Width, oldDisplayedRectangle.Height);

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(baseImage, new Rectangle(0, 0, oldDisplayedRectangle.Width, oldDisplayedRectangle.Height), rect, GraphicsUnit.Pixel);
                }

                DateTime dt1 = DateTime.Now;

                label1.Text += '\n'.ToString() + (dt1 - dt0).TotalMilliseconds.ToString(); 

                if (label1.Text.Length > 350)
                {
                    label1.Text = "";
                }

                pictureBox1.Image.Dispose();
                pictureBox1.Image = bmp;
                pictureBox1.Invalidate();

                dX = e.X;
                dY = e.Y;
                //oldDisplayedRectangle = rect;
            }

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
                pictureBox2.Visible = false;
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
                label2.Text = Path.GetFileName(path);
                label2.Top = 5;
                label2.Left = resolution.Width / 2 - label2.Width;
                label2.Visible = true;
                label2.BackColor = Color.FromArgb(65, 33, 33, 33);
                pictureBox2.Visible = false;
            }
            else
            {
                label2.Visible = false;
                pictureBox2.Visible = false;
            }
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
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Default;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            loadImage = Manipulator.ZoomImage(ed, ex, ey, baseImage, ref oldDisplayedRectangle, scale, resolution, 1);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pictureBox1.Image = loadImage;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.SizeAll;
            dX = e.X;
            dY = e.Y;
        }
    }
}
