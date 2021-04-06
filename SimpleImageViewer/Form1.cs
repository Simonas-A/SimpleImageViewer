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
        //Image scaledImage;
        int scale = 0;

        DateTime time0 = DateTime.Now;
        DateTime time1 = DateTime.Now;

        Rectangle resolution;

        private void Form1_Load(object sender, EventArgs e)
        {
            //MessageBox.Show("Rel");
            monitor = SimpleImageViewer.Properties.Settings.Default.Monitor;

            resolution = Screen.AllScreens[monitor].Bounds;

            label1.Text = "";
            //label2.Text = "";


            //BackColor = Color.FromArgb(33, 33, 33);
            this.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);




            string[] args = Environment.GetCommandLineArgs();


            //string[] args = new string[] { "", "E:\\Joniniu Foto 2020\\DSC_3314.jpg" };
            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\DSC_3314.jpg" }; //portrait
            //args = new string[] { "", "C:\\Users\\Simuxxl\\Desktop\\DSC_3432.jpg" }; //landscape
            //args = new string[] { "", "G:\\PicturesBackup 2020-12-16\\Camera\\20171014_121602.jpg" }; //port
            //args = new string[] { "", "G:\\PicturesBackup 2020-12-16\\Camera\\20171014_123121.jpg" }; // land

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

            
            foreach (string arg in args)
            {
                MessageBox.Show(arg);
            }
            */


            if (args.Length > 1)
            {
                path = args[1];

                //time0 = DateTime.Now;
                //IntPtr image = IntPtr.Zero;
                //int status = SafeNativeMethods.Gdip.GdipLoadImageFromFile(path, out image);
                //Image img = CreateImageObject(image);
                Image img = Image.FromFile(args[1]);
                //Image img = (Image)Bitmap.FromFile(args[1]);

                //var stream = File.OpenRead(args[1]);
                //Image img = Image.FromStream(stream);

                //baseImage = img;

                //time1 = DateTime.Now;

                //MessageBox.Show((time1 - time0).ToString());

                string supportedExtensions = "*.jpg,*.png,*.bmp,*.jpe,*.jpeg";
                string root = Path.GetDirectoryName(path);

                //this.BackColor = Color.FromArgb(33, 33, 33);

                //pictureBox2.Image = Image.FromFile("C:\\Users\\Simuxxl\\Desktop\\cover5.jpg");
                //MessageBox.Show("");

                this.Size = new Size(resolution.Width, resolution.Height);
                pictureBox1.BackColor = Color.FromArgb(33, 33, 33);
                pictureBox1.Size = new Size(resolution.Width, resolution.Height);
                this.Location = new Point(resolution.X, resolution.Y);

                label2.Left = resolution.Width - 30;
                label2.Top = 5;
                //(img);


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
                    //pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                }


                baseImage = img;

                //SetFullImage(img, resolution);
                pictureBox1.Image = img;
                //pictureBox1.Image = new Bitmap(img);
                //img.Dispose();
                //SetFullImage(img, resolution);
                //SetImage(img);
                //Thread thread = new Thread(SetImage(img));
                //thread.Start();
                files = Directory.GetFiles(root, "*.*").Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            }
            //MessageBox.Show(sender.ToString());
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            /*
            double zoomFactor = 0.1;
            Size newSize = new Size((int)(pictureBox1.Image.Width * zoomFactor), (int)(pictureBox1.Image.Height * zoomFactor));
            Bitmap bmp = new Bitmap(pictureBox1.Image, newSize);
            pictureBox1.Image = bmp;

            */



            /*
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            if (e.Delta > 0 && scale < 48)
            {
                double factor = 0.5;
                int incX = (int)(pictureBox1.Width * factor);
                int incY = (int)(pictureBox1.Height * factor);

                double right = (double)e.X / pictureBox1.Width;
                double left = 1 - (double)e.X / pictureBox1.Width;

                pictureBox1.Width += (int)(incX * right);
                pictureBox1.Height += incY;
                pictureBox1.Left -= (int)((incX / 2) * left);
                pictureBox1.Top -= incY / 2;

                scale++;
            }
            else if (e.Delta < 0 && scale > 0)
            {
                if (scale == 1)
                {
                    SetImage(baseImage);
                }
                else
                {
                    double factor = 0.25;
                    int incX = (int)(pictureBox1.Width * factor);
                    int incY = (int)(pictureBox1.Height * factor);

                    pictureBox1.Left += incX / 2;
                    pictureBox1.Top += incY / 2;
                    pictureBox1.Width -= incX;
                    pictureBox1.Height -= incY;
                    
                }
                scale--;
            }


            label1.Text = pictureBox1.Location.ToString() + Environment.NewLine + pictureBox1.Size + Environment.NewLine + scale;
            */


            //ResizeAndDisplayImage();
            //UpdateZoomedImage(e);
            /*
            int sclOld = scale;
            if (e.Delta > 0 && scale < 24)
            {
                scale++;
            }
            else if (e.Delta < 0 && scale > 0)
            {
                scale--;
            }
            

            if (scale != 0)
            {
                if (sclOld != scale)
                {
                    DateTime dt = DateTime.Now;
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    //pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;

                    //Size newSize = new Size((int)(baseImage.Width * ((float)scale / 6)), (int)(baseImage.Height * ((float)scale / 6)));
                    //Bitmap bmp = new Bitmap(baseImage, newSize);

                    //pictureBox1.Image = bmp;



                    int X = e.X;
                    int Y = e.Y;
                    Point size = new Point(pictureBox1.ClientSize.Width * (25 - scale) / 24, pictureBox1.ClientSize.Height * (25 - scale) / 24);

                    Point startPoint = new Point(X - size.X / 2 < 0 ? 0 : X - size.X / 2, Y - size.Y / 2 < 0 ? 0 : Y - size.Y / 2);
                    int width = X + size.X / 2 > pictureBox1.ClientSize.Width ? pictureBox1.ClientSize.Width - startPoint.X : X + size.X / 2 - startPoint.X;
                    int height = Y + size.Y / 2 > pictureBox1.ClientSize.Height ? pictureBox1.ClientSize.Height - startPoint.Y : Y + size.Y / 2 - startPoint.Y;

                    pictureBox1.Left = -startPoint.X;
                    pictureBox1.Top = -startPoint.Y;

                    pictureBox1.Width = pictureBox1.Width + 1 * startPoint.X;
                    pictureBox1.Height = pictureBox1.Height + 1 * startPoint.Y;

                    //scaledImage = (Image)(((Bitmap)baseImage).Clone(new Rectangle(startPoint, new Size(width, height)), System.Drawing.Imaging.PixelFormat.Format24bppRgb));
                    scaledImage = baseImage;

                    //pictureBox1.Image = scaledImage;
                    DateTime dt1 = DateTime.Now;
                    //MessageBox.Show((dt1 - dt).TotalMilliseconds.ToString());
                    //MessageBox.Show(pictureBox1.Location.ToString() + Environment.NewLine + pictureBox1.Size);
                    label1.Text = size.ToString() + Environment.NewLine + pictureBox1.Location.ToString() + Environment.NewLine + pictureBox1.Size + Environment.NewLine + scale;
                    //label1.Text = "img: " + baseImage.Size.ToString() + Environment.NewLine + "pctr: " + pictureBox1.ClientSize.ToString();
                }
            }
            else
            {
                pictureBox1.Left = 0;
                pictureBox1.Top = 0;
                pictureBox1.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                SetFullImage(baseImage, Screen.PrimaryScreen.Bounds);
            }



            */
        }

        private void SetImage(Image img)
        {
            pictureBox1.Size = Screen.AllScreens[monitor].Bounds.Size;
            pictureBox1.Location = new Point(0, 0);

            //Rectangle resolution = Screen.AllScreens[monitor].Bounds;

            //Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort();

            //Task.Factory.StartNew(() => SetThumbnail( (Image)(new Bitmap(img, resolution.Width / 10, resolution.Height / 10)), img, resolution));



            SetFullImage(img);
            //img.Dispose();
        }

        private void SetThumbnail(Image img, Image fullImg)
        {
            //MessageBox.Show(img.Size.ToString());
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox1.Image = img;

            Task.Factory.StartNew(() => SetFullImage(fullImg));

        }

        private void SetFullImage(Image img)
        {
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
            //ExifReader reader = new ExifReader(path);
            //reader.GetTagValue(ExifTags.Orientation, out int orientation);
            //MessageBox.Show(orientation.ToString());

            //Bitmap bmp = new Bitmap(img);
            if (img.Width < resolution.Width && img.Height < resolution.Height)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            else
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                //pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            }

            pictureBox1.Image = img;
            //img.Dispose();
        }

        private void SetFullImage(Bitmap bmp)
        {
            pictureBox1.Image.Dispose();
            if (bmp.PropertyIdList.Contains(0x0112))
            {
                int orientation = bmp.GetPropertyItem(0x0112).Value[0];

                if (orientation == 6)
                {
                    bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                }
                else if (orientation == 8)
                {
                    bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                }
                else if (orientation != 1)
                {
                    //MessageBox.Show("Unknown orientation: " + orientation);
                }

            }
            //ExifReader reader = new ExifReader(path);
            //reader.GetTagValue(ExifTags.Orientation, out int orientation);
            //MessageBox.Show(orientation.ToString());

            //Bitmap bmp = new Bitmap(img);
            if (bmp.Width < resolution.Width && bmp.Height < resolution.Height)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            else
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }

            pictureBox1.Image = bmp;
            //bmp.Dispose();
            //img.Dispose();
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
                scale = 0;
                //DateTime time0 = DateTime.Now;
                //time0 = DateTime.Now;
                ImageNext();
                //DateTime time1 = DateTime.Now;
                //MessageBox.Show((time1 - time0).ToString());
            }
            else if (e.KeyCode == Keys.R)
            {
                scale = 0;
                ImageRandom();
            }
            else if (e.KeyCode == Keys.Q)
            {
                
            }
        }

        private void ImageNext()
        {
            //pictureBox1.Image.Dispose();
            //string supportedExtensions = "*.jpg,*.png,*.bmp,*.jpe,*.jpeg";
            //string[] files = Directory.GetFiles(root, "*.*").Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();

            int id = Array.IndexOf(files, path);


            //MessageBox.Show((time1 - time0).ToString());
            if (id < files.Length - 1)
            {
                //time0 = DateTime.Now;
                Image baseImage = Image.FromFile(files[id + 1]);
                //time1 = DateTime.Now;

                path = files[id + 1];
                //time1 = DateTime.Now;

                //MessageBox.Show((time1 - time0).ToString());

                //Task.Factory.StartNew(() => SetImage(img));

                //pictureBox1.Image = new Bitmap(baseImage);
                //baseImage.Dispose();
                SetImage(baseImage);
                //baseImage.Dispose();
            }
            //baseImage.Dispose();

        }

        private void ImagePrevious()
        {
            /*
            string root = Path.GetDirectoryName(path);
            string supportedExtensions = "*.jpg,*.png,*.bmp,*.jpe,*.jpeg";
            string[] files = Directory.GetFiles(root, "*.*").Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            */
            int id = Array.IndexOf(files, path);

            if (id > 0)
            {
                Image baseImage = Image.FromFile(files[id - 1]);
                path = files[id - 1];
                //Task.Factory.StartNew(() => SetImage(img));
                SetImage(baseImage);
                // baseImage.Dispose();
            }
        }

        private void ImageRandom()
        {
            Random rnd = new Random();
            int id = rnd.Next(files.Length);
            path = files[id];
            Image baseImage = Image.FromFile(files[id]);
            SetImage(baseImage);

        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            pictureBox1.Size = Screen.AllScreens[monitor].Bounds.Size;
            pictureBox1.Location = new Point(0, 0);
            scale = 0;
            //Close();
            //SetImage(baseImage);
        }

        private void ResizeAndDisplayImage()
        {
            /*
            // Set the backcolor of the pictureboxes

            Color _BackColor = Color.FromArgb(33, 33, 33);

            pictureBox1.BackColor = _BackColor;
            //picZoom.BackColor = _BackColor;

            // If baseImage is null, then return. This situation can occur

            // when a new backcolor is selected without an image loaded.

            if (baseImage == null)
                return;

            // sourceWidth and sourceHeight store
            // the original image's width and height

            // targetWidth and targetHeight are calculated
            // to fit into the pictureBox1 picturebox.

            int sourceWidth = baseImage.Width;
            int sourceHeight = baseImage.Height;
            int targetWidth;
            int targetHeight;
            double ratio;

            // Calculate targetWidth and targetHeight, so that the image will fit into

            // the pictureBox1 picturebox without changing the proportions of the image.

            if (sourceWidth > sourceHeight)
            {
                // Set the new width

                targetWidth = pictureBox1.Width;
                // Calculate the ratio of the new width against the original width

                ratio = (double)targetWidth / sourceWidth;
                // Calculate a new height that is in proportion with the original image

                targetHeight = (int)(ratio * sourceHeight);
            }
            else if (sourceWidth < sourceHeight)
            {
                // Set the new height

                targetHeight = pictureBox1.Height;
                // Calculate the ratio of the new height against the original height

                ratio = (double)targetHeight / sourceHeight;
                // Calculate a new width that is in proportion with the original image

                targetWidth = (int)(ratio * sourceWidth);
            }
            else
            {
                // In this case, the image is square and resizing is easy

                targetHeight = pictureBox1.Height;
                targetWidth = pictureBox1.Width;
            }

            // Calculate the targetTop and targetLeft values, to center the image

            // horizontally or vertically if needed

            int targetTop = (pictureBox1.Height - targetHeight) / 2;
            int targetLeft = (pictureBox1.Width - targetWidth) / 2;

            // Create a new temporary bitmap to resize the original image

            // The size of this bitmap is the size of the pictureBox1 picturebox.

            Bitmap tempBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height, PixelFormat.Format24bppRgb);

            // Set the resolution of the bitmap to match the original resolution.

            tempBitmap.SetResolution(baseImage.HorizontalResolution, baseImage.VerticalResolution);

            // Create a Graphics object to further edit the temporary bitmap

            Graphics bmGraphics = Graphics.FromImage(tempBitmap);

            // First clear the image with the current backcolor

            bmGraphics.Clear(_BackColor);

            // Set the interpolationmode since we are resizing an image here

            bmGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Draw the original image on the temporary bitmap, resizing it using

            // the calculated values of targetWidth and targetHeight.

            bmGraphics.DrawImage(baseImage,new Rectangle(targetLeft, targetTop, targetWidth, targetHeight), new Rectangle(0, 0, sourceWidth, sourceHeight), GraphicsUnit.Pixel);

            // Dispose of the bmGraphics object

            bmGraphics.Dispose();

            // Set the image of the pictureBox1 picturebox to the temporary bitmap

            pictureBox1.Image = tempBitmap;
            */
        }

        private void UpdateZoomedImage(MouseEventArgs e)
        {
            // Calculate the width and height of the portion of the image we want

            // to show in the picZoom picturebox. This value changes when the zoom

            // factor is changed.
            int _ZoomFactor = scale / 6;

            int zoomWidth = pictureBox1.Width / _ZoomFactor;
            int zoomHeight = pictureBox1.Height / _ZoomFactor;

            // Calculate the horizontal and vertical midpoints for the crosshair

            // cursor and correct centering of the new image

            int halfWidth = zoomWidth / 2;
            int halfHeight = zoomHeight / 2;

            // Create a new temporary bitmap to fit inside the picZoom picturebox

            Bitmap tempBitmap = new Bitmap(zoomWidth, zoomHeight,
                                           PixelFormat.Format24bppRgb);

            // Create a temporary Graphics object to work on the bitmap

            Graphics bmGraphics = Graphics.FromImage(tempBitmap);

            // Clear the bitmap with the selected backcolor

            bmGraphics.Clear(Color.FromArgb(33, 33, 33));

            // Set the interpolation mode

            bmGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Draw the portion of the main image onto the bitmap

            // The target rectangle is already known now.

            // Here the mouse position of the cursor on the main image is used to

            // cut out a portion of the main image.

            bmGraphics.DrawImage(pictureBox1.Image,
                                 new Rectangle(0, 0, zoomWidth, zoomHeight),
                                 new Rectangle(e.X - halfWidth, e.Y - halfHeight,
                                 zoomWidth, zoomHeight), GraphicsUnit.Pixel);

            // Draw the bitmap on the picZoom picturebox

            pictureBox1.Image = tempBitmap;

            // Draw a crosshair on the bitmap to simulate the cursor position

            bmGraphics.DrawLine(Pens.Black, halfWidth + 1,
                                halfHeight - 4, halfWidth + 1, halfHeight - 1);
            bmGraphics.DrawLine(Pens.Black, halfWidth + 1, halfHeight + 6,
                                halfWidth + 1, halfHeight + 3);
            bmGraphics.DrawLine(Pens.Black, halfWidth - 4, halfHeight + 1,
                                halfWidth - 1, halfHeight + 1);
            bmGraphics.DrawLine(Pens.Black, halfWidth + 6, halfHeight + 1,
                                halfWidth + 3, halfHeight + 1);

            // Dispose of the Graphics object

            bmGraphics.Dispose();

            // Refresh the picZoom picturebox to reflect the changes

            pictureBox1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //label1.Text = e.X + Environment.NewLine + e.Y;
            /*
            //MessageBox.Show("DWH");
            Bitmap bmp = new Bitmap(1920, 1080, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {

                g.FillRectangle(new SolidBrush(Color.Transparent), 0, 0, 1920, 1080);
                g.Clear(Color.Transparent);

                Pen pen = new Pen(Color.Orange, 4);
                g.DrawRectangle(pen, e.X - 100, e.Y - 75, 200, 150);
                //pictureBox2.Invalidate();
            }
            pictureBox2.Image = bmp;
            */

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
                SetImage(baseImage);

                this.Size = new Size(resolution.Width, resolution.Height);
                pictureBox1.Size = new Size(resolution.Width, resolution.Height);
                this.Location = new Point(resolution.X, resolution.Y);
            }
            else if (X <= reactionSize && Y <= reactionSize) // top left
            {
                WindowState = FormWindowState.Minimized;
            }
        }
    }
}
