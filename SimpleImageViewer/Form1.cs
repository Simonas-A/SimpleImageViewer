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


        int monitor = 1;

        string path = "";

        string[] files;

        Image baseImage;
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
                        MessageBox.Show("Unknown orientation: " + orientation);
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
                pictureBox1.Image = img;

                oldDisplayedRectangle = new Rectangle(0, 0, img.Width, img.Height);
                files = Directory.GetFiles(root, "*.*").Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            }
        }

        private Rectangle GetDisplayedRectangle(Image img, Rectangle resolution)
        {
            img = pictureBox1.Image;
            int x;
            int y;
            int width;
            int height;

            if ( (img.Width < resolution.Width && img.Height < resolution.Height) && !(pictureBox1.SizeMode == PictureBoxSizeMode.Zoom))
            {
                x = Convert.ToInt32(pictureBox1.Width * 0.5 - pictureBox1.Image.Width * 0.5);
                y = Convert.ToInt32(pictureBox1.Height * 0.5 - pictureBox1.Image.Height * 0.5);
                width = img.Width;
                height = img.Height;
            }
            else
            {
                if ((double)img.Width/resolution.Width > (double)img.Height/resolution.Height)
                {
                    double ratio = (double)img.Width / resolution.Width;
                    x = 0;
                    y = Convert.ToInt32((resolution.Height - (img.Height / ratio)) * 0.5);
                    width = resolution.Width;
                    height = Convert.ToInt32(img.Height / ratio);
                }
                else
                {
                    double ratio = (double)img.Height / resolution.Height;
                    x = Convert.ToInt32((resolution.Width - (img.Width / ratio)) * 0.5);
                    y = 0;
                    width = Convert.ToInt32(img.Width / ratio);
                    height = resolution.Height;
                }
            }
            return new Rectangle(x, y, width, height);
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            pictureBox1.Image = ZoomImage(e.Delta, e.X, e.Y, baseImage);
        }

        private Image ZoomImage(int delta, int eX, int eY, Image baseImage)
        {
            double zoomFactor = 0.85;

            DateTime dt0 = DateTime.Now;

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            if (delta != 0)
                scale += delta / Math.Abs(delta);

            label1.Text = "Scale: " + scale.ToString();

            if (scale <= 0)
            {
                scale = 0;
                pictureBox1.Image = baseImage;
                oldDisplayedRectangle = new Rectangle(0, 0, baseImage.Width, baseImage.Height);

                return baseImage;
            }

            Rectangle displayRectangle = GetDisplayedRectangle(pictureBox1.Image, resolution);
            label1.Text += '\n'.ToString() + displayRectangle.ToString();


            double imageRatio = (double)baseImage.Width / baseImage.Height;
            double displayRatio = (double)resolution.Width / resolution.Height;

            int width, height;

            if (baseImage.Width / resolution.Width > baseImage.Height / resolution.Height)
            {//Wide picture

                width = Convert.ToInt32(baseImage.Width * Math.Pow(zoomFactor, scale));
                height = Convert.ToInt32(width / displayRatio);
            }
            else
            {//Tall picture

                height = Convert.ToInt32(baseImage.Height * Math.Pow(zoomFactor, scale));
                width = Convert.ToInt32(height * displayRatio);
            }

            label1.Text += '\n'.ToString() + "Width: " + width.ToString() + "; Height: " + height.ToString();

            int x = oldDisplayedRectangle.X + ((oldDisplayedRectangle.Width - width)) * eX / (resolution.Width - displayRectangle.X);
            int y = oldDisplayedRectangle.Y + ((oldDisplayedRectangle.Height - height)) * eY / (resolution.Height - displayRectangle.Y);

            if (delta < 0)
            {

                if (x + width > baseImage.Width)
                {
                    x = baseImage.Width - width;
                }

                if (y + height > baseImage.Height)
                {
                    y = baseImage.Height - height;
                }

                if (x < 0)
                {
                    x = 0;

                    if (x + width > baseImage.Width)
                    {
                        x = (baseImage.Width - width) / 2;
                    }
                }

                if (y < 0)
                {
                    y = 0;

                    if (y + height > baseImage.Height)
                    {
                        y = (baseImage.Height - height) / 2;
                    }
                }
            }
            else
            {
                if (x < 0 || x + width > baseImage.Width)
                {
                    x = 0;
                    width = baseImage.Width;
                }

                if (y < 0 || y + height > baseImage.Height)
                {
                    y = 0;
                    height = baseImage.Height;
                }
            }

            oldDisplayedRectangle = new Rectangle(x, y, width, height);

            label1.Text += "\n newDims: " + oldDisplayedRectangle.ToString();

            DateTime dt1 = DateTime.Now;

            Bitmap bmp = new Bitmap(oldDisplayedRectangle.Width, oldDisplayedRectangle.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(baseImage, new Rectangle(0, 0, oldDisplayedRectangle.Width, oldDisplayedRectangle.Height), oldDisplayedRectangle, GraphicsUnit.Pixel);
            }

            DateTime dt2 = DateTime.Now;


            DateTime dt3 = DateTime.Now;

            label1.Text = "Calculations: " + (dt1 - dt0).TotalMilliseconds.ToString() + "\nCloning: " + (dt2 - dt1).TotalMilliseconds.ToString() + "\nDisplaying: " + (dt3 - dt2).TotalMilliseconds.ToString();

            return bmp;
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
                    MessageBox.Show("Unknown orientation: " + orientation);
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


            label2.Text = Path.GetFileName(path);
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
            oldDisplayedRectangle = new Rectangle(0, 0, baseImage.Width, baseImage.Height);

            if (baseImage.Width < resolution.Width && baseImage.Height < resolution.Height)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //pictureBox1.Image = ZoomImage(0, e.X, e.Y, baseImage);
                oldDisplayedRectangle.X += (dX - e.X) * oldDisplayedRectangle.Width / resolution.Width;
                oldDisplayedRectangle.Y += (dY - e.Y) * oldDisplayedRectangle.Height / resolution.Height;

                Bitmap bmp = new Bitmap(oldDisplayedRectangle.Width, oldDisplayedRectangle.Height);

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(baseImage, new Rectangle(0, 0, oldDisplayedRectangle.Width, oldDisplayedRectangle.Height), oldDisplayedRectangle, GraphicsUnit.Pixel);
                }

                pictureBox1.Image.Dispose();
                pictureBox1.Image = bmp;
                pictureBox1.Invalidate();

                dX = e.X;
                dY = e.Y;

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

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.SizeAll;
            dX = e.X;
            dY = e.Y;
        }
    }
}
